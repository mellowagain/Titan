using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using Titan.Json;
using Titan.MatchID.Live;
using Titan.Meta;

namespace Titan.Account
{
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

        ////////////////////////////////////////////////////
        // TICKS
        ////////////////////////////////////////////////////

        public long StartTick { get; set; }

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

        // ReSharper disable once InconsistentNaming
        public ReportInfo _reportInfo; // Setting this to private will cause it not to be visible for inheritated classes

        public void FeedReportInfo(ReportInfo info)
        {
            _reportInfo = info;
        }
        
        // ReSharper disable once InconsistentNaming
        public CommendInfo _commendInfo; // Setting this to private will cause it not to be visible for inheritated classes

        public void FeedCommendInfo(CommendInfo info)
        {
            _commendInfo = info;
        }

        // ReSharper disable once InconsistentNaming
        public LiveGameInfo _liveGameInfo; // Setting this to private will cause it not to be visible for inheritated classes

        public void FeedLiveGameInfo(LiveGameInfo info)
        {
            _liveGameInfo = info;
        }

        public MatchInfo MatchInfo;

        ////////////////////////////////////////////////////
        // PAYLOADS
        ////////////////////////////////////////////////////

        public ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportPlayer> GetReportPayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportPlayer>(
                (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientReportPlayer
            );

            payload.Body.account_id = _reportInfo.SteamID.AccountID;
            payload.Body.match_id = _reportInfo.MatchID;
            payload.Body.rpt_aimbot = (uint) (_reportInfo.AimHacking ? 2 : 0);
            payload.Body.rpt_wallhack = (uint) (_reportInfo.WallHacking ? 3 : 0);
            payload.Body.rpt_speedhack = (uint) (_reportInfo.OtherHacking ? 4 : 0);
            payload.Body.rpt_teamharm = (uint) (_reportInfo.Griefing ? 5 : 0);
            payload.Body.rpt_textabuse = (uint) (_reportInfo.AbusiveText ? 6 : 0);
            payload.Body.rpt_voiceabuse = (uint) (_reportInfo.AbusiveVoice ? 7 : 0);

            return payload;
        }

        public ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientCommendPlayer> GetCommendPayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientCommendPlayer>(
                (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientCommendPlayer
            );

            payload.Body.account_id = _commendInfo.SteamID.AccountID;
            payload.Body.match_id = 0;

            payload.Body.commendation = new PlayerCommendationInfo
            {
                cmd_friendly = (uint) (_commendInfo.Friendly ? 1 : 0),
                cmd_teaching = (uint) (_commendInfo.Teacher ? 2 : 0),
                cmd_leader = (uint) (_commendInfo.Leader ? 4 : 0)
            };

            payload.Body.tokens = 10; // Whatever this is

            return payload;
        }

        public ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchListRequestLiveGameForUser> GetLiveGamePayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchListRequestLiveGameForUser>(
                (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchListRequestLiveGameForUser
            );

            payload.Body.accountid = _liveGameInfo.SteamID.AccountID;
            
            return payload;
        }

    }
}
