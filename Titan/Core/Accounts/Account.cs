using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using log4net;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.Internal;

namespace Titan.Core.Accounts
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

                SteamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = _username,
                    Password = _password
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
            Log.Debug("Successfully disconncected from Steam.");
            IsRunning = false;
        }

        public void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if(callback.Result == EResult.OK)
            {
                // Success
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
            }
            else if(callback.Result == EResult.AccountLogonDenied)
            {
                Log.Warn("Account has Steam Guard enabled.");
                // Steam Guard enabled
                IsRunning = false;
            }
            else
            {
                Log.ErrorFormat("Unable to logon to account: {0}: {1}", callback.Result, callback.ExtendedResult);
                IsRunning = false;
            }
        }

        public void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Log.DebugFormat("Successfully logged off from Steam: {0}", callback.Result);
        }

        public void OnGcMessage(SteamGameCoordinator.MessageCallback callback)
        {
            var map = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { (uint) EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientReportResponse, OnReportResponse },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientCommendPlayerQueryResponse, OnReportResponse }
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