using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using log4net;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.Internal;
using Titan.Bot.Mode;

namespace Titan.Bot.Threads
{
    public class Account
    {
        public readonly ILog Log;

        private readonly string _username;
        private readonly string _password;

        public BotMode Mode { get; set; }
        public uint Target { get; set; }
        public ulong MatchID { get; set; }

        // TODO: Steam Guard handleing
        public readonly DirectoryInfo SentryDirectory;
        public readonly FileInfo SentryFile;

        public string AuthCode { get; set; }
        public string TwoFactorCode { get; set; }

        [DefaultValue(0)]
        public int ReconnectTries { get; private set; }

        public Json.Accounts.JsonAccount Json { get; set; }

        public SteamClient SteamClient { get; private set; }
        public SteamUser SteamUser { get; private set; }
        public SteamGameCoordinator GameCoordinator { get; private set; }
        public CallbackManager Callbacks { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsSuccess { get; private set; }

        public Account(string username, string password, Json.Accounts.JsonAccount json)
        {
            _username = username;
            _password = password;

            ReconnectTries = 0;

            Json = json;

            SentryDirectory = new DirectoryInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sentries");
            SentryFile = new FileInfo(Path.Combine(SentryDirectory.ToString(), username + ".sentry.bin"));

            Log = LogManager.GetLogger("Account - " + username);

            SteamClient = new SteamClient();
            Callbacks = new CallbackManager(SteamClient);
            SteamUser = SteamClient.GetHandler<SteamUser>();
            GameCoordinator = SteamClient.GetHandler<SteamGameCoordinator>();
        }

        public void Process()
        {
            Thread.CurrentThread.Name = _username + " - " + Mode;

            Log.Debug("Connecting to Steam");

            Callbacks.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            Callbacks.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            Callbacks.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            Callbacks.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            Callbacks.Subscribe<SteamGameCoordinator.MessageCallback>(OnGcMessage);
            Callbacks.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            IsRunning = true;
            SteamClient.Connect();

            while(IsRunning)
            {
                Callbacks.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        // ==========================================
        // CALLBACKS
        // ==========================================

        public void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if(callback.Result == EResult.OK)
            {
                Log.Debug("Successfully connected to Steam. Logging in...");

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
                Log.ErrorFormat("Unable to connect to Steam: {0}", callback.Result);
                IsRunning = false;
            }
        }

        public void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            ReconnectTries++;
            if(ReconnectTries <= 5 && !IsSuccess)
            {
                Log.DebugFormat("Disconnected from Steam. Retrying in 5 seconds... ({0}/5)", ReconnectTries);

                Thread.Sleep(TimeSpan.FromSeconds(5));

                SteamClient.Connect();
            }
            else
            {
                Log.Debug("Successfully disconnected from Steam.");
                IsRunning = false;
            }
        }

        public void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            switch(callback.Result)
            {
                case EResult.OK:
                    Log.Debug("Successfully logged in. Registering that we're playing CS:GO...");

                    var playGames = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
                    playGames.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                    {
                        game_id = 730
                    });
                    SteamClient.Send(playGames);

                    Thread.Sleep(5000);

                    Log.Debug("Successfully registered playing CS:GO. Sending client hello to CS:GO services.");

                    var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint) EGCBaseClientMsg.k_EMsgGCClientHello);
                    GameCoordinator.Send(clientHello, 730);
                    break;
                case EResult.AccountLogonDenied:
                    Log.Info("Please enter the 2FA code from the Steam App:");
                    AuthCode = Console.ReadLine();
                    break;
                case EResult.AccountLoginDeniedNeedTwoFactor:
                    Log.InfoFormat("Please enter the auth code sent to email at {0}:", callback.EmailDomain);
                    TwoFactorCode = Console.ReadLine();
                    break;
                case EResult.ServiceUnavailable:
                    Log.Error("Steam is currently offline. Please try again later.");
                    IsRunning = false;
                    break;
                default:
                    Log.ErrorFormat("Unable to logon to account: {0}: {1}", callback.Result, callback.ExtendedResult);
                    IsRunning = false;
                    break;
            }
        }

        public void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Log.DebugFormat("Successfully logged off from Steam: {0}", callback.Result);
        }

        public void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Log.Debug("Updating Sentry file...");

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

            Log.Debug("Successfully updated Sentry hash file.");
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
            Log.DebugFormat("Successfully received client hello from CS:GO services. Sending {0}...", Mode);

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

            Log.DebugFormat("Successfully reported. Confirmation ID: {0}", response.Body.confirmation_id);

            IsSuccess = true;

            SteamUser.LogOff();
            SteamClient.Disconnect();
        }

        public void OnCommendResponse(IPacketGCMsg msg)
        {
            Log.Debug("Successfully commended target.");

            IsSuccess = true;

            SteamUser.LogOff();
            SteamClient.Disconnect();
        }

    }
}