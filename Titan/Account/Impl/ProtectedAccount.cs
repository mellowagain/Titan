using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
using Titan.Mode;
using Titan.UI;
using Titan.UI._2FA;

namespace Titan.Account.Impl
{
    public class ProtectedAccount : TitanAccount
    {
        
        //////////////////////////////////////////////////////////////
        // TODO: This class seems to be broken. See GH Issue #8 
        //////////////////////////////////////////////////////////////

        private Logger _log;

        private int _reconnects;

        private FileInfo _file;

        private SteamGuardAccount _sgAccount;
        private string _authCode;
        private string _2FactorCode;

        private SteamClient _steamClient;
        private SteamUser _steamUser;
        private SteamFriends _steamFriends;
        private SteamGameCoordinator _gameCoordinator;
        private CallbackManager _callbacks;

        public Result Result { get; private set; }
        public bool IsRunning { get; private set; }

        public ProtectedAccount(JsonAccounts.JsonAccount json) : base(json)
        {
            _log = LogCreator.Create("GC - " + json.Username + " (Protected)");

            _file = new FileInfo(Path.Combine(Path.Combine(Environment.CurrentDirectory, "sentries"),
                json.Username + ".sentry"));

            _steamClient = new SteamClient();
            _callbacks = new CallbackManager(_steamClient);
            _steamUser = _steamClient.GetHandler<SteamUser>();
            _steamFriends = _steamClient.GetHandler<SteamFriends>();
            _gameCoordinator = _steamClient.GetHandler<SteamGameCoordinator>();

            if(json.SharedSecret != null)
            {
                _sgAccount = new SteamGuardAccount
                {
                    SharedSecret = json.SharedSecret
                };
            }

            _log.Debug("Successfully initialized account object for " + json.Username + ".");
        }

        public override Result Start()
        {
            Thread.CurrentThread.Name = JsonAccount.Username + " - " + _info.Mode;

            _callbacks.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbacks.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            _callbacks.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbacks.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            _callbacks.Subscribe<SteamGameCoordinator.MessageCallback>(OnGCMessage);
            _callbacks.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

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
                _log.Debug("Sentry has been activated for this account. Checking if a sentry file " +
                           "exists and hashing it...");

                byte[] sentryHash = null;
                if(_file.Exists)
                {
                    _log.Debug("Sentry file found. Hashing...");

                    var fileBytes = File.ReadAllBytes(_file.ToString());
                    sentryHash = CryptoHelper.SHAHash(fileBytes);

                    _log.Debug("Hash for sentry file found: {Hash}", Encoding.UTF8.GetString(sentryHash));
                }

                _steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = JsonAccount.Username,
                    Password = JsonAccount.Password,
                    AuthCode = _authCode,
                    TwoFactorCode = _2FactorCode,
                    SentryFileHash = sentryHash
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

            if(_reconnects <= 5 && (Result != Result.Success ||
               Result != Result.AlreadyLoggedInSomewhereElse || IsRunning))
            {
                _log.Information("Disconnected from Steam. Retrying in 5 seconds... ({Count}/5)", _reconnects);

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
                    _log.Information("Opening UI form to get the 2FA Steam Guard App Code...");

                    if(_sgAccount != null)
                    {
                        _log.Debug("A shared secret has been provided: automaticly generating it...");
                        
                        _2FactorCode = _sgAccount.GenerateSteamGuardCode();
                    }
                    else
                    {

                        Application.Instance.Invoke(() => Titan.Instance.UIManager.ShowForm(
                            UIType.TwoFactorAuthentification,
                            new TwoFactorAuthForm(Titan.Instance.UIManager, this, null)));

                        while(_2FactorCode == null)
                        {
                            /* Wait until the Form inputted the 2FA code from the Steam Guard App */
                        }
                        
                    }

                    _log.Information("Received 2FA Code: {Code}", _2FactorCode);
                    break;
                case EResult.AccountLogonDenied:
                    _log.Information("Opening UI form to get the Auth Token from EMail...");

                    Application.Instance.Invoke(() => Titan.Instance.UIManager.ShowForm(UIType.TwoFactorAuthentification,
                        new TwoFactorAuthForm(Titan.Instance.UIManager, this, callback.EmailDomain)));

                    while(_authCode == null)
                    {
                        /* Wait until the Form inputted the Auth code from the Email Steam sent */
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
            _log.Debug("Checking if a sentry file exists...");

            int size;
            byte[] hash;

            using(var fwr = File.Open(_file.ToString(), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fwr.Seek(callback.Offset, SeekOrigin.Begin);
                fwr.Write(callback.Data, 0, callback.BytesToWrite);
                size = (int) fwr.Length;

                fwr.Seek(0, SeekOrigin.Begin);
                using(var sha = SHA1.Create())
                {
                    hash = sha.ComputeHash(fwr);
                }
            }

            _log.Debug("Successfully opened / created sentry file. Hash: {Hash}", Encoding.UTF8.GetString(hash));

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

            switch(_info.Mode)
            {
                case BotMode.Report:
                    _gameCoordinator.Send(GetReportPayload(_info.Target, _info.MatchID), 730);
                    break;
                case BotMode.Commend:
                    _gameCoordinator.Send(GetCommendPayload(_info.Target), 730);
                    break;
                case BotMode.RemoveCommend:
                    _gameCoordinator.Send(GetRemoveCommendPayload(_info.Target), 730);
                    break;
            }
        }

        public override void OnReportResponse(IPacketGCMsg msg)
        {
            var response = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportResponse>(msg);

            switch(_info.Mode)
            {
                case BotMode.Report:
                    _log.Information("Successfully reported. Confirmation ID: {Id}", response.Body.confirmation_id);
                    break;
                case BotMode.Commend:
                    _log.Information("Successfully commended {Target} with a Leader, Friendly and a Teacher.",
                        _info.Target);
                    break;
                case BotMode.RemoveCommend:
                    _log.Information("Successfully removed Leader, Friendly an Teacher commends from {Target}.",
                        _info.Target);
                    break;
            }

            Result = Result.Success;

            Stop();
        }

        public override void OnCommendResponse(IPacketGCMsg msg)
        {
            _log.Information("Successfully " + (_info.Mode == BotMode.RemoveCommend ? "un" : "") + 
                             "commended target {Target}.", _info.Target);

            Result = Result.Success;

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