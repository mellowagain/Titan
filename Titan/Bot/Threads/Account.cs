using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Serilog.Core;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.Internal;
using Titan.Bot.Mode;
using Titan.Json;
using Titan.Logging;

namespace Titan.Bot.Threads
{
    public class Account
    {

        private Logger _log;

        private readonly string _username;
        private readonly string _password;

        public BotMode Mode { get; set; }
        public uint Target { get; set; }
        public ulong MatchID { get; set; }

        public readonly DirectoryInfo SentryDirectory;
        public readonly FileInfo SentryFile;

        public string AuthCode { get; set; }
        public string TwoFactorCode { get; set; }

        [DefaultValue(0)]
        public int ReconnectTries { get; private set; }

        public JsonAccounts.JsonAccount Json { get; set; }

        public SteamClient SteamClient { get; private set; }
        public SteamUser SteamUser { get; private set; }
        public SteamFriends SteamFriends { get; private set; }
        public SteamGameCoordinator GameCoordinator { get; private set; }
        public CallbackManager Callbacks { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsSuccess { get; private set; }
        public bool IsLoggedInSomewhereElse { get; private set; }

        public Account(JsonAccounts.JsonAccount json)
        {
            _username = json.Username;
            _password = json.Password;
            Json = json;

            ReconnectTries = 0;

            SentryDirectory = new DirectoryInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sentries");
            SentryFile = new FileInfo(Path.Combine(SentryDirectory.ToString(), json.Username + ".sentry.bin"));

            _log = LogCreator.Create("GC - " + json.Username);

            SteamClient = new SteamClient();
            Callbacks = new CallbackManager(SteamClient);
            SteamUser = SteamClient.GetHandler<SteamUser>();
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
            GameCoordinator = SteamClient.GetHandler<SteamGameCoordinator>();
        }

        public bool Process()
        {
            Thread.CurrentThread.Name = _username + " - " + Mode;

            _log.Debug("Connecting to Steam");

            Callbacks.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            Callbacks.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            Callbacks.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            Callbacks.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            Callbacks.Subscribe<SteamGameCoordinator.MessageCallback>(OnGcMessage);
            if(Json.Sentry) { Callbacks.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth); }

            IsRunning = true;
            SteamClient.Connect();

            while(IsRunning)
            {
                Callbacks.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            return IsSuccess;
        }

        public void Stop()
        {
            if(SteamFriends.GetPersonaState() == EPersonaState.Online)
            {
                SteamFriends.SetPersonaState(EPersonaState.Offline);
            }

            if(SteamUser.SteamID != null)
            {
                SteamUser.LogOff();
            }

            if(SteamClient.IsConnected)
            {
                SteamClient.Disconnect();
            }
        }

        // ==========================================
        // CALLBACKS
        // ==========================================

        public void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if(callback.Result == EResult.OK)
            {
                _log.Debug("Successfully connected to Steam. Logging in...");

                if(Json.Sentry)
                {
                    byte[] sentryHash = null;
                    if(SentryFile.Exists)
                    {
                        var fileBytes = File.ReadAllBytes(SentryFile.ToString());
                        sentryHash = CryptoHelper.SHAHash(fileBytes);
                    }
                    SteamUser.LogOn(new SteamUser.LogOnDetails
                    {
                        Username = _username,
                        Password = _password,
                        AuthCode = this.AuthCode,
                        TwoFactorCode = this.TwoFactorCode,
                        SentryFileHash = sentryHash
                    });
                }
                else
                {
                    SteamUser.LogOn(new SteamUser.LogOnDetails
                    {
                        Username = _username,
                        Password = _password
                    });
                }
            }
            else
            {
                _log.Error("Unable to connect to Steam: {Result}", callback.Result);
                IsRunning = false;
            }
        }

        public void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            ReconnectTries++;
            if(ReconnectTries <= 5 && !IsSuccess && !IsLoggedInSomewhereElse || !IsRunning )
            {
                _log.Debug("Disconnected from Steam. Retrying in 5 seconds... ({Count}/5)", ReconnectTries);

                Thread.Sleep(TimeSpan.FromSeconds(5));

                SteamClient.Connect();
            }
            else
            {
                _log.Debug("Successfully disconnected from Steam.");
                IsRunning = false;
            }
        }

        public void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            switch(callback.Result)
            {
                case EResult.OK:
                    _log.Debug("Successfully logged in. Registering that we're playing CS:GO...");

                    SteamFriends.SetPersonaState(EPersonaState.Online);

                    var playGames = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
                    playGames.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                    {
                        game_id = 730
                    });
                    SteamClient.Send(playGames);

                    Thread.Sleep(5000);

                    _log.Debug("Successfully registered playing CS:GO. Sending client hello to CS:GO services.");

                    var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint) EGCBaseClientMsg.k_EMsgGCClientHello);
                    GameCoordinator.Send(clientHello, 730);
                    break;
                case EResult.AccountLogonDenied:
                    if(Json.Sentry)
                    {
                        _log.Information("Please enter the 2FA code from the Steam App:");
                        AuthCode = Console.ReadLine();
                    }
                    break;
                case EResult.AccountLoginDeniedNeedTwoFactor:
                    if(Json.Sentry)
                    {
                        _log.Information("Please enter the auth code sent to email at {Mail}:", callback.EmailDomain);
                        TwoFactorCode = Console.ReadLine();
                    }
                    break;
                case EResult.ServiceUnavailable:
                    _log.Error("Steam is currently offline. Please try again later.");
                    Stop();
                    IsRunning = false;
                    break;
                default:
                    _log.Error("Unable to logon to account: {Result}: {ExtendedResult}", callback.Result, callback.ExtendedResult);
                    Stop();
                    IsRunning = false;
                    break;
            }
        }

        public void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            IsLoggedInSomewhereElse = callback.Result == EResult.LoggedInElsewhere
                || callback.Result == EResult.AlreadyLoggedInElsewhere;
            if(IsLoggedInSomewhereElse)
                _log.Warning("Account is already logged on somewhere else.");
            else
                _log.Debug("Successfully logged off from Steam: {Result}", callback.Result);
        }

        public void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            if(Json.Sentry)
            {
                _log.Debug("Updating Sentry file...");

                int size;
                byte[] hash;
                using(var fwr = File.Open(SentryFile.ToString(), FileMode.OpenOrCreate, FileAccess.ReadWrite))
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

                SteamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
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

                _log.Debug("Successfully updated Sentry hash file.");
            }
        }

        public void OnGcMessage(SteamGameCoordinator.MessageCallback callback)
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

        public void OnClientWelcome(IPacketGCMsg msg)
        {
            _log.Debug("Successfully received client hello from CS:GO services. Sending {Mode}...", Mode);

            if(Mode == BotMode.Report)
            {
                var sendReport =
                    new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportPlayer>((uint) ECsgoGCMsg
                        .k_EMsgGCCStrike15_v2_ClientReportPlayer);
                sendReport.Body.account_id = Target;
                sendReport.Body.match_id = MatchID;
                sendReport.Body.rpt_aimbot = 2;
                sendReport.Body.rpt_wallhack = 3;
                sendReport.Body.rpt_speedhack = 4;
                sendReport.Body.rpt_teamharm = 5;
                sendReport.Body.rpt_textabuse = 6;
                sendReport.Body.rpt_voiceabuse = 7;
                GameCoordinator.Send(sendReport, 730);
            }
            else
            {
                var commend =
                    new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientCommendPlayer>((uint) ECsgoGCMsg
                        .k_EMsgGCCStrike15_v2_ClientCommendPlayer);
                commend.Body.account_id = Target;
                commend.Body.match_id = 8;
                commend.Body.commendation = new PlayerCommendationInfo
                {
                    cmd_friendly = 1,
                    cmd_teaching = 2,
                    cmd_leader = 4
                };
                commend.Body.tokens = 10;
                GameCoordinator.Send(commend, 730);
            }
        }

        public void OnReportResponse(IPacketGCMsg msg)
        {
            var response = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportResponse>(msg);

            _log.Information("Successfully reported. Confirmation ID: {Id}", response.Body.confirmation_id);

            IsSuccess = true;

            Stop();
        }

        public void OnCommendResponse(IPacketGCMsg msg)
        {
            _log.Information("Successfully commended target {Target}.", Target);

            IsSuccess = true;

            Stop();
        }

    }
}