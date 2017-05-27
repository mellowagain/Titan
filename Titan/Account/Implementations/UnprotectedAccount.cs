using System;
using System.Collections.Generic;
using System.Threading;
using Serilog.Core;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.Internal;
using Titan.Json;
using Titan.Logging;
using Titan.Mode;

namespace Titan.Account.Implementations
{
    public class UnprotectedAccount : TitanAccount
    {

        private Logger _log;

        private int _reconnects;

        private SteamClient _steamClient;
        private SteamUser _steamUser;
        private SteamFriends _steamFriends;
        private SteamGameCoordinator _gameCoordinator;
        private CallbackManager _callbacks;

        public Result Result { get; private set; }
        public bool IsRunning { get; private set; }

        public UnprotectedAccount(JsonAccounts.JsonAccount json) : base(json)
        {
            _log = LogCreator.Create("GC - " + json.Username);

            _steamClient = new SteamClient();
            _callbacks = new CallbackManager(_steamClient);
            _steamUser = _steamClient.GetHandler<SteamUser>();
            _steamFriends = _steamClient.GetHandler<SteamFriends>();
            _gameCoordinator = _steamClient.GetHandler<SteamGameCoordinator>();

            _log.Debug("Successfully initialized account object for {0}.", json.Username);
        }

        public override Result Start()
        {
            Thread.CurrentThread.Name = JsonAccount.Username + " - " + _info.Mode;

            _callbacks.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbacks.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            _callbacks.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbacks.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            _callbacks.Subscribe<SteamGameCoordinator.MessageCallback>(OnGCMessage);

            IsRunning = true;
            _steamClient.Connect();

            while(IsRunning)
            {
                _callbacks.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            return Result;
        }

        public override void Stop()
        {
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
        }

        ////////////////////////////////////////////////////
        // CALLBACKS
        ////////////////////////////////////////////////////

        public override void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if(callback.Result == EResult.OK)
            {
                _log.Debug("Successfully connected to Steam. Logging in...");

                _steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = JsonAccount.Username,
                    Password = JsonAccount.Password
                });
            }
            else
            {
                _log.Error("Unable to connect to Steam: {Result}", callback.Result);
                IsRunning = false;
            }
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

                    var banInfo = Titan.Instance.BanManager.GetBanInfoFor(_steamUser.SteamID.ConvertToUInt64());
                    if(banInfo != null && (banInfo.VacBanned || banInfo.GameBanCount > 0))
                    {
                        _log.Warning("The account has a ban on record. " +
                                     "If the VAC/Game ban ban is from CS:GO, a {Mode} is not possible. " +
                                     "Proceeding with caution.", _info.Mode.ToString().ToLower());
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
                case EResult.AccountLogonDenied:
                    _log.Debug("Two Factor Authentification is activated on this account. Please set " +
                               "Sentry to {true} in the accounts.json for this account.", true);

                    Stop();

                    IsRunning = false;
                    Result = Result.SentryRequired;
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

        public override void OnGCMessage(SteamGameCoordinator.MessageCallback callback)
        {
            var map = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { (uint) EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientReportResponse, OnReportResponse },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientCommendPlayerQueryResponse, OnCommendResponse }
            };

            Action<IPacketGCMsg> func;
            if(map.TryGetValue(callback.EMsg, out func))
            {
                func(callback.Message);
            }
        }

        public override void OnClientWelcome(IPacketGCMsg msg)
        {
            _log.Debug("Successfully received client hello from CS:GO services. Sending {Mode}...", _info.Mode);

            if(_info.Mode == BotMode.Report)
                _gameCoordinator.Send(GetReportPayload(_info.Target, _info.MatchID), 730);
            else
                _gameCoordinator.Send(GetCommendPayload(_info.Target), 730);
        }

        public override void OnReportResponse(IPacketGCMsg msg)
        {
            var response = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportResponse>(msg);

            if(_info.Mode == BotMode.Report)
            {
                _log.Information("Successfully reported. Confirmation ID: {Id}", response.Body.confirmation_id);
            }
            else
            {
                _log.Information("Successfully commended {Target} with a Leader, Friendly and a Teacher.",
                    _info.Target);
            }

            Result = Result.Success;

            Stop();
        }

        public override void OnCommendResponse(IPacketGCMsg msg)
        {
            _log.Information("Successfully commended target {Target}.", _info.Target);

            Result = Result.Success;

            Stop();
        }

    }
}