using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using Titan.Json;

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

        ////////////////////////////////////////////////////
        // BOTTING SESSION INFORMATIONS
        ////////////////////////////////////////////////////

        // ReSharper disable once InconsistentNaming
        public Info _info; // Setting this to private will cause it not to be visible for inheritated classes

        public void Feed(Info info)
        {
            _info = info;
        }

        ////////////////////////////////////////////////////
        // PAYLOADS
        ////////////////////////////////////////////////////

        public ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportPlayer> GetReportPayload(uint target, ulong match)
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportPlayer>(
                (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientReportPlayer);

            payload.Body.account_id = target;
            payload.Body.match_id = match;
            payload.Body.rpt_aimbot = 2;
            payload.Body.rpt_wallhack = 3;
            payload.Body.rpt_speedhack = 4;
            payload.Body.rpt_teamharm = 5;
            payload.Body.rpt_textabuse = 6;
            payload.Body.rpt_voiceabuse = 7;

            return payload;
        }

        public ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientCommendPlayer> GetCommendPayload(uint target)
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientCommendPlayer>(
                (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientCommendPlayer);

            payload.Body.account_id = target;
            payload.Body.match_id = 8;

            payload.Body.commendation = new PlayerCommendationInfo
            {
                cmd_friendly = 1,
                cmd_teaching = 2,
                cmd_leader = 4
            };

            payload.Body.tokens = 10; // Whatever this is

            return payload;
        }

    }
}