using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using Titan.Json;
using Titan.MatchID.Live;
using Titan.Meta;

namespace Titan.Account
{
    
    // ReSharper disable InconsistentNaming
    public abstract class TitanAccount
    {

        ////////////////////////////////////////////////////
        // JSON SPECIFICATIONS
        ////////////////////////////////////////////////////

        public JsonAccounts.JsonAccount JsonAccount;

        internal TitanAccount(JsonAccounts.JsonAccount json)
        {
            JsonAccount = json;
        }

        public bool IsLast = false;

        ////////////////////////////////////////////////////
        // TICKS
        ////////////////////////////////////////////////////

        public long StartTick { get; set; }
        
        public bool IsRunning { get; set; }

        ////////////////////////////////////////////////////
        // GENERAL
        ////////////////////////////////////////////////////

        public abstract Result Start();
        public abstract void Stop();

        public abstract void OnConnected(SteamClient.ConnectedCallback callback);
        public abstract void OnDisconnected(SteamClient.DisconnectedCallback callback);

        public abstract void OnLoggedOn(SteamUser.LoggedOnCallback callback);
        public abstract void OnLoggedOff(SteamUser.LoggedOffCallback callback);

        ////////////////////////////////////////////////////
        // GAME COORDINATOR
        ////////////////////////////////////////////////////

        public abstract void OnGCMessage(SteamGameCoordinator.MessageCallback callback);

        public abstract void OnClientWelcome(IPacketGCMsg msg);
        public abstract void OnReportResponse(IPacketGCMsg msg);
        public abstract void OnCommendResponse(IPacketGCMsg msg);
        public abstract void OnLiveGameRequestResponse(IPacketGCMsg msg);

        ////////////////////////////////////////////////////
        // BOTTING SESSION INFORMATIONS
        ////////////////////////////////////////////////////

        public ReportInfo _reportInfo; // Setting this to private will cause it not to be visible for inheritated classes

        public void FeedReportInfo(ReportInfo info)
        {
            _reportInfo = info;
        }
        
        public CommendInfo _commendInfo; // Setting this to private will cause it not to be visible for inheritated classes

        public void FeedCommendInfo(CommendInfo info)
        {
            _commendInfo = info;
        }

        public LiveGameInfo _liveGameInfo; // Setting this to private will cause it not to be visible for inheritated classes
        public MatchInfo MatchInfo;

        public void FeedLiveGameInfo(LiveGameInfo info)
        {
            _liveGameInfo = info;
        }

        public IdleInfo _idleInfo;

        public void FeedIdleInfo(IdleInfo info)
        {
            _idleInfo = info;
        }

        ////////////////////////////////////////////////////
        // PAYLOADS
        ////////////////////////////////////////////////////

        public ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportPlayer> GetReportPayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportPlayer>(
                (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientReportPlayer
            )
            {
                Body =
                {
                    account_id = _reportInfo.SteamID.AccountID,
                    match_id = _reportInfo.MatchID,
                    rpt_aimbot = (uint) (_reportInfo.AimHacking ? 1 : 0),
                    rpt_wallhack = (uint) (_reportInfo.WallHacking ? 1 : 0),
                    rpt_speedhack = (uint) (_reportInfo.OtherHacking ? 1 : 0),
                    rpt_teamharm = (uint) (_reportInfo.Griefing ? 1 : 0),
                    rpt_textabuse = (uint) (_reportInfo.AbusiveText ? 1 : 0),
                    rpt_voiceabuse = (uint) (_reportInfo.AbusiveVoice ? 1 : 0)
                }
            };

            return payload;
        }

        public ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientCommendPlayer> GetCommendPayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientCommendPlayer>(
                (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientCommendPlayer
            )
            {
                Body =
                {
                    account_id = _commendInfo.SteamID.AccountID,
                    match_id = 0,
                    commendation = new PlayerCommendationInfo
                    {
                        cmd_friendly = (uint) (_commendInfo.Friendly ? 1 : 0),
                        cmd_teaching = (uint) (_commendInfo.Teacher ? 1 : 0),
                        cmd_leader = (uint) (_commendInfo.Leader ? 1 : 0)
                    },
                    tokens = 0
                }
            };

            return payload;
        }

        public ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchListRequestLiveGameForUser> GetLiveGamePayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchListRequestLiveGameForUser>(
                (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchListRequestLiveGameForUser
            ) 
            {
                Body =
                {
                    accountid = _liveGameInfo.SteamID.AccountID
                }
            };

            return payload;
        }

    }
}
