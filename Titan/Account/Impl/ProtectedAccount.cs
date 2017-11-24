using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Eto.Forms;
using Serilog.Core;
using SteamAuth;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.Internal;
using Titan.Json;
using Titan.Logging;
using Titan.MatchID.Live;
using Titan.UI;
using Titan.UI._2FA;
using Titan.Util;
using Titan.Web;

namespace Titan.Account.Impl
{
    public class ProtectedAccount : TitanAccount
    {

        private Logger _log;

        private int _reconnects;

        private SteamConfiguration _steamConfig;
        private Sentry.Sentry _sentry;
        
        private SteamGuardAccount _sgAccount;
        private string _authCode;
        private string _2FactorCode;

        private SteamClient _steamClient;
        private SteamUser _steamUser;
        private SteamFriends _steamFriends;
        private SteamGameCoordinator _gameCoordinator;
        private CallbackManager _callbacks;
        private TitanHandler _titanHandle;

        public Result Result { get; private set; } = Result.Unknown;

        public ProtectedAccount(JsonAccounts.JsonAccount json) : base(json)
        {
            _log = LogCreator.Create("GC - " + json.Username + (!Titan.Instance.Options.Secure ? " (Protected)" : ""));

            _steamConfig = new SteamConfiguration
            {
                ConnectionTimeout = TimeSpan.FromMinutes(3),
                WebAPIKey = Titan.Instance.WebHandle.GetKey() // May be null at this time, but we can accept that for now
            };
            
            _sentry = new Sentry.Sentry(this);
            
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

            if(json.SharedSecret != null)
            {
                _sgAccount = new SteamGuardAccount
                {
                    SharedSecret = json.SharedSecret
                };
            }

            _log.Debug("Successfully initialized account object for " + json.Username + ".");
        }

        ~ProtectedAccount()
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
            _callbacks.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            IsRunning = true;
            _steamClient.Connect();

            while (IsRunning)
            {
                _callbacks.RunWaitAllCallbacks(TimeSpan.FromMilliseconds(500));
            }

            return Result;
        }

        public override void Stop()
        {
            _reportInfo = null;
            _commendInfo = null;
            _liveGameInfo = null;
            _idleInfo = null;
            
            if(_steamFriends.GetPersonaState() == EPersonaState.Online)
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
            _log.Debug("Received on connected: {@callback} - Job ID: {id}", callback, callback.JobID.Value);
            
            byte[] hash = null;
            if (_sentry.Exists())
            {
                _log.Debug("Found previous Sentry file. Hashing it and sending it to Steam...");

                hash = _sentry.Hash();
            }
            else
            {
                _log.Debug("No Sentry file found. Titan will ask for a confirmation code...");
            }
            
            _log.Debug("Logging in with Auth Code: {a} / 2FA Code: {b} / Hash: {c}", _authCode, _2FactorCode, 
                hash != null ? Convert.ToBase64String(hash) : null);
            _steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = JsonAccount.Username,
                Password = JsonAccount.Password,
                AuthCode = _authCode,
                TwoFactorCode = _2FactorCode,
                SentryFileHash = hash,
                LoginID = RandomUtil.RandomUInt32()
            });
        }

        public override void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            _reconnects++;

            if(_reconnects <= 5 && !callback.UserInitiated &&  IsRunning)
            {
                // TODO: Fix reconnecting, see GH issue
                _log.Information("Disconnected from Steam. Retrying in 2 seconds... ({Count}/5)", _reconnects);
                _log.Debug("Steam: {@steam} - Account: {@this}", _steamClient, this);

                Thread.Sleep(TimeSpan.FromSeconds(2));
                
                var worker = new BackgroundWorker();
                worker.DoWork += (sender, args) =>
                {
                    _log.Debug("Starting watchdog.");
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    
                    _log.Debug("Steam Client connected after 10 sec: {bool}", _steamClient.IsConnected);
                    _log.Debug("Steam: {@steam} - Account: {@this}", _steamClient, this);
                };
                worker.RunWorkerAsync();
                
                _log.Debug("Reconnecting to Steam. Watchdog: {@worker}", worker);
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
                    playGames.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                    {
                        game_id = 730
                    });
                    _steamClient.Send(playGames);

                    Thread.Sleep(5000);

                    _log.Debug("Successfully registered playing CS:GO. Sending client hello to CS:GO services.");

                    var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint) EGCBaseClientMsg.k_EMsgGCClientHello);
                    _gameCoordinator.Send(clientHello, 730);
                    break;
                case EResult.AccountLoginDeniedNeedTwoFactor:
                    if(_sgAccount != null)
                    {
                        _log.Debug("A shared secret has been provided: automatically generating it...");
                        
                        _2FactorCode = _sgAccount.GenerateSteamGuardCode();
                    }
                    else
                    {
                        _log.Information("Opening UI form to get the 2FA Steam Guard App Code...");

                        Application.Instance.Invoke(() => Titan.Instance.UIManager.ShowForm(
                            UIType.TwoFactorAuthentification,
                            new TwoFactorAuthForm(Titan.Instance.UIManager, this, null)));

                        while(string.IsNullOrEmpty(_2FactorCode))
                        {
                            /* Wait until we receive the Steam Guard code from the UI */
                        }
                    }

                    _log.Information("Received 2FA Code: {Code}", _2FactorCode);
                    break;
                case EResult.AccountLogonDenied:
                    _log.Information("Opening UI form to get the Auth Token from EMail...");

                    Application.Instance.Invoke(() => Titan.Instance.UIManager.ShowForm(UIType.TwoFactorAuthentification,
                        new TwoFactorAuthForm(Titan.Instance.UIManager, this, callback.EmailDomain)));

                    while(string.IsNullOrEmpty(_authCode))
                    {
                        /* Wait until we receive the Auth Token from the UI */
                    }

                    _log.Information("Received Auth Token: {Code}", _authCode);
                    break;
                case EResult.ServiceUnavailable:
                    _log.Error("Steam is currently offline. Please try again later.");

                    Stop();

                    IsRunning = false;
                    break;
                case EResult.RateLimitExceeded:
                    _log.Debug("Steam Rate Limit has been reached. Please try it again in a few minutes...");

                    Stop();

                    IsRunning = false;
                    Result = Result.RateLimit;
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

        public void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            _log.Debug("Updating Steam sentry file...");

            if (_sentry.Save(callback.Offset, callback.Data, callback.BytesToWrite, out var hash, out var size))
            {
                _steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
                {
                    JobID = callback.JobID,
                    FileName = callback.FileName,
                    
                    BytesWritten = callback.BytesToWrite,
                    FileSize = size,
                    Offset = callback.Offset,
                    
                    Result = EResult.OK,
                    LastError = 0,
                    
                    OneTimePassword = callback.OneTimePassword,
                    
                    SentryFileHash = hash
                });
                
                _log.Information("Successfully updated Steam sentry file.");
            }
            else
            {
                _log.Error("Could not save sentry file. Titan will ask again for Steam Guard code on next attempt.");
            }
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

        public void FeedWithAuthToken(string authToken)
        {
            _authCode = authToken;
        }

        public void FeedWith2FACode(string twofactorCode)
        {
            _2FactorCode = twofactorCode;
        }

    }
}
