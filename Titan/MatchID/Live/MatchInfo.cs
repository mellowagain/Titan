using System.Collections.Generic;
using Newtonsoft.Json;
using SteamKit2.GC.CSGO.Internal;

namespace Titan.MatchID.Live
{
    public class MatchInfo
    {

        [JsonProperty("matchid")]
        public ulong MatchID;

        [JsonProperty("matchtime")]
        public uint MatchTime;

        [JsonProperty("watchablematchinfo")]
        public WatchableMatchInfo WatchableMatchInfo;

        [JsonProperty("roundstats")]
        public List<CMsgGCCStrike15_v2_MatchmakingServerRoundStats> RoundsStats;

    }
}