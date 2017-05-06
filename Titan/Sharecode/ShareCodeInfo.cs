using Newtonsoft.Json;

namespace Titan.Sharecode
{
    public class ShareCodeInfo
    {

        [JsonProperty("matchId")]
        public ulong MatchId { get; set; }

        [JsonProperty("outcomeId")]
        public ulong OutcomeId { get; set; }

        [JsonProperty("token")]
        public short Token { get; set; }

    }
}