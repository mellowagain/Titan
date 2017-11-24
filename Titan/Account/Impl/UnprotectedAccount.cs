using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Serilog.Core;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.Internal;
using Titan.Json;
using Titan.Logging;
using Titan.MatchID.Live;
using Titan.UI;
using Titan.UI.General;
using Titan.Util;
using Titan.Web;

namespace Titan.Account.Impl
{
    
    public class UnprotectedAccount : TitanAccount, IJob
    {

        private Logger _log;

        private int _reconnects;

        private SteamConfiguration _steamConfig;
        
        private SteamClient _steamClient;
        private SteamUser _steamUser;
        private SteamFriends _steamFriends;
        private SteamGameCoordinator _gameCoordinator;
        private CallbackManager _callbacks;
        private TitanHandler _titanHandle;

        public Result Result { get; private set; } = Result.Unknown;

        public IJobDetail Job;
        public ITrigger Trigger;

        public UnprotectedAccount(JsonAccounts.JsonAccount json) : base(json)
        {
            _log = LogCreator.Create("GC - " + json.Username + (!Titan.Instance.Options.Secure ? " (Unprotected)" : ""));

            _steamConfig = new SteamConfiguration
            {
                ConnectionTimeout = TimeSpan.FromMinutes(1),
                WebAPIKey = Titan.Instance.WebHandle.GetKey() // May be null at this time, but we can accept that for now
            };
            
            _steamClient = new SteamClient(_steamConfig);
            _callbacks = new CallbackManager(_steamClient);
            _steamUser = _steamClient.GetHandler<SteamUser>();
            _steamFriends = _steamClient.GetHandler<SteamFriends>();
            _gameCoordinator = _steamClient.GetHandler<SteamGameCoordinator>();
            
            _titanHandle = new TitanHandler();
            _steamClient.AddHandler(_titanHandle);
            
            // Initialize debug network sniffer when debug mode is enabled
            if(Titan.Instance.Options.Debug)
            {
                var dir = new DirectoryInfo(Path.Combine(Titan.Instance.DebugDirectory.ToString(), json.Username));
                if(!dir.Exists)
                {
                    dir.Create();
                }
                
                _steamClient.DebugNetworkListener = new NetHookNetworkListener(
                    dir.ToString()
                );
            }

            Job = JobBuilder.Create<UnprotectedAccount>()
                .WithIdentity("Idle Job - " + json.Username + " (Unprotected)", "Titan")
                .Build();

            _log.Debug("Successfully initialized account object for {Username}.", json.Username);
        }

        ~UnprotectedAccount()
        {
            if(IsRunning)
            {
                Stop();
            }
        }

        public override Result Start()
        {
            Thread.CurrentThread.Name = JsonAccount.Username + " - " + (_reportInfo != null ? "Report" : "Commend");

            _callbacks.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbacks.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            _callbacks.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbacks.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            _callbacks.Subscribe<SteamGameCoordinator.MessageCallback>(OnGCMessage);

            IsRunning = true;
            _steamClient.Connect();

            while (IsRunning)
            {
                _callbacks.RunWaitCallbacks(TimeSpan.FromMilliseconds(500));
            }

            return Result;
        }

        public override void Stop()
        {
            _reportInfo = null;
            _commendInfo = null;
            _liveGameInfo = null;
            _idleInfo = null;
            
            if(_steamFriends.GetPersonaState() != EPersonaState.Offline)
            {
                _steamFriends.SetPersonaState(EPersonaState.Offline);
            }

            if(_steamUser.SteamID != null)
            {
                _steamUser.LogOff();
            }

            if(_steamClient.IsConnected)
            {
                _steamClient.Disconnect();
            }

            IsRunning = false;
            
            Titan.Instance.ThreadManager.FinishBotting(this);
        }

        ////////////////////////////////////////////////////
        // CALLBACKS
        ////////////////////////////////////////////////////

        public override void OnConnected(SteamClient.ConnectedCallback callback)
        {
            _log.Debug("Successfully connected to Steam. Logging in...");

            _steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = JsonAccount.Username,
                Password = JsonAccount.Password,
                LoginID = RandomUtil.RandomUInt32()
            });
        }

        public override void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            _reconnects++;

            if(_reconnects <= 5 && (Result != Result.Success &&
               Result != Result.AlreadyLoggedInSomewhereElse || IsRunning))
            {
                _log.Debug("Disconnected from Steam. Retrying in 5 seconds... ({Count}/5)", _reconnects);

                Thread.Sleep(TimeSpan.FromSeconds(5));

                _steamClient.Connect();
            }
            else
            {
                _log.Debug("Successfully disconnected from Steam.");
                IsRunning = false;
            }
        }

        public override void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            switch(callback.Result)
            {
                case EResult.OK:
                    _log.Debug("Successfully logged in. Checking for any VAC or game bans...");

                    var banInfo = Titan.Instance.WebHandle.RequestBanInfo(_steamUser.SteamID.ConvertToUInt64());
                    if(banInfo != null && (banInfo.VacBanned || banInfo.GameBanCount > 0))
                    {
                        _log.Warning("The account has a ban on record. " +
                                     "If the VAC/Game ban ban is from CS:GO, a {Mode} is not possible. " +
                                     "Proceeding with caution.", _reportInfo != null ? "report" :"commend");
                        Result = Result.AccountBanned;
                    }

                    _log.Debug("Registering that we're playing CS:GO...");

                    _steamFriends.SetPersonaState(EPersonaState.Online);

                    var playGames = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
                    if(_idleInfo != null)
                    {
                        foreach(var gameID in _idleInfo.GameID)
                        {
                            var gamesPlayed = new CMsgClientGamesPlayed.GamePlayed
                            {
                                game_id = Convert.ToUInt64(gameID)
                            };

                            if(gameID == 0)
                            {
                                gamesPlayed.game_extra_info = Titan.Instance.UIManager.GetForm<General>(UIType.General)
                                    .CustomGameName;
                            }
                            
                            playGames.Body.games_played.Add(gamesPlayed);
                        }
                    }
                    else
                    {
                        playGames.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                        {
                            game_id = 730
                        });
                    }
                    _steamClient.Send(playGames);

                    Thread.Sleep(5000);


                    if(_idleInfo == null)
                    {
                        _log.Debug("Successfully registered playing CS:GO. Sending client hello to CS:GO services.");

                        var clientHello =
                            new ClientGCMsgProtobuf<CMsgClientHello>((uint) EGCBaseClientMsg.k_EMsgGCClientHello);
                        _gameCoordinator.Send(clientHello, 730);
                    }
                    else
                    {
                        Trigger = TriggerBuilder.Create()
                            .WithIdentity("Idle Trigger - " + JsonAccount.Username + " (Unprotected)", "Titan")
                            .StartNow()
                            .WithSimpleSchedule(x => x
                                .WithIntervalInMinutes(_idleInfo.Minutes)
                                .WithRepeatCount(1))
                            .Build();

                        Titan.Instance.Scheduler.ScheduleJob(Job, Trigger);

                        _idleInfo.StartTick = DateTime.Now.Ticks;
                        
                        _log.Debug("Successfully registered idling in requested games. Starting scheduler.");
                    }
                    break;
                case EResult.AccountLoginDeniedNeedTwoFactor:
                case EResult.AccountLogonDenied:
                    _log.Debug("Two Factor Authentification is activated on this account. Please set " +
                               "Sentry to {true} in the accounts.json for this account.", true);

                    Stop();

                    IsRunning = false;
                    Result = Result.SentryRequired;
                    break;
                case EResult.InvalidPassword:
                case EResult.NoConnection:
                case EResult.Timeout:
                case EResult.TryAnotherCM:
                case EResult.TwoFactorCodeMismatch:
                case EResult.ServiceUnavailable:
                    _log.Error("Unable to connect to Steam: {Reason}. Retrying...", callback.ExtendedResult);
                    
                    break;
                case EResult.RateLimitExceeded:
                    _log.Debug("Steam Rate Limit has been reached. Please try it again in a few minutes...");

                    Stop();

                    IsRunning = false;
                    Result = Result.RateLimit;
                    break;
                case EResult.AccountDisabled:
                    _log.Error("This account has been permanently disabled by the Steam network.");
                    
                    Stop();

                    IsRunning = false;
                    Result = Result.AccountBanned;
                    break;
                default:
                    _log.Error("Unable to logon to account: {Result}: {ExtendedResult}", callback.Result, callback.ExtendedResult);
                    
                    Stop();
                    IsRunning = false;
                    break;
            }
        }

        public override void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            if(callback.Result == EResult.LoggedInElsewhere || callback.Result == EResult.AlreadyLoggedInElsewhere)
                Result = Result.AlreadyLoggedInSomewhereElse;

            if(Result == Result.AlreadyLoggedInSomewhereElse)
                _log.Warning("Account is already logged on somewhere else. Skipping...");
            else
                _log.Debug("Successfully logged off from Steam: {Result}", callback.Result);
        }

        public override void OnGCMessage(SteamGameCoordinator.MessageCallback callback)
        {
            var map = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { (uint) EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientReportResponse, OnReportResponse },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientCommendPlayerQueryResponse, OnCommendResponse },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchList, OnLiveGameRequestResponse }
            };

            if(map.TryGetValue(callback.EMsg, out var func))
            {
                func(callback.Message);
            }
        }

        public override void OnClientWelcome(IPacketGCMsg msg)
        {
            _log.Debug("Successfully received client hello from CS:GO services. Sending {Mode}...",
                _liveGameInfo != null ? "Live Game Request" : (_reportInfo != null ? "Report" : "Commend"));
            
            if(_liveGameInfo != null)
            {
                _gameCoordinator.Send(GetLiveGamePayload(), 730);
            }
            else if(_reportInfo != null)
            {
                _gameCoordinator.Send(GetReportPayload(), 730);
            }
            else
            {
                _gameCoordinator.Send(GetCommendPayload(), 730);
            }
        }

        public override void OnReportResponse(IPacketGCMsg msg)
        {
            var response = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportResponse>(msg);

            if(_reportInfo != null)
            {
                _log.Information("Successfully reported. Confirmation ID: {ID}", response.Body.confirmation_id);
            }
            else
            {
                _log.Information("Successfully commended {Target} with {Pretty}.",
                    _commendInfo.SteamID.ConvertToUInt64(), _commendInfo.ToPrettyString());
            }

            Result = Result.Success;

            Stop();
        }

        public override void OnCommendResponse(IPacketGCMsg msg)
        {
            _log.Information("Successfully commended target {Target} with {Pretty}.", 
                _commendInfo.SteamID.ConvertToUInt64(), _commendInfo.ToPrettyString());

            Result = Result.Success;

            Stop();
        }

        public override void OnLiveGameRequestResponse(IPacketGCMsg msg)
        {
            var response = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchList>(msg);

            if(response.Body.matches.Count >= 1)
            {
                var matchInfos = response.Body.matches.Select(match => new MatchInfo
                    {
                        MatchID = match.matchid,
                        MatchTime = match.matchtime,
                        WatchableMatchInfo = match.watchablematchinfo,
                        RoundsStats = match.roundstatsall
                    }
                ).ToList();

                MatchInfo = matchInfos[0]; // TODO: Maybe change this into a better than meme than just using the 0 index

                _log.Information("Received live game Match ID: {MatchID}", MatchInfo.MatchID);

                Result = Result.Success;
            }
            else
            {
                MatchInfo = new MatchInfo
                {
                    MatchID = 8,
                    MatchTime = 0,
                    WatchableMatchInfo = null,
                    RoundsStats = null
                };
                
                Result = Result.NoMatches;
            }
            
            Stop();
        }
        
        ////////////////////////////////////////////////////
        // QUARTZ.NET SCHEDULER
        ////////////////////////////////////////////////////

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                var difference = DateTime.Now.Subtract(new DateTime(_idleInfo.StartTick));
            
                // Quartz.NET may be off by 2 minutes. We accept that (2 minutes less idling wow, what are you gonna do, cry?)
                if(difference.Minutes >= _idleInfo.Minutes || _idleInfo.Minutes - difference.Minutes <= 2) 
                {
                    _log.Information("Successfully finished idling after {Minutes} minutes.", _idleInfo.Minutes);

                    Result = Result.Success;
                    Stop();
                }
                else
                {
                    _log.Debug("Quartz.NET requested stop of idling already after {Minutes} minutes.", difference.Minutes);
                
                    _log.Debug("Continueing idling for {Remaining} minutes.", _idleInfo.Minutes - difference.Minutes);
                }
            });
        }
        
    }
}