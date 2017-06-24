using Newtonsoft.Json;

namespace Titan.Sharecode
{
    public class ShareCodeInfo
    {

        [JsonProperty("matchid")]
        public ulong MatchID { get; set; }
        
        [JsonProperty("outcomeid")]
        public ulong OutcomeID { get; set; }
        
        [JsonProperty("tokens")]
        public uint Tokens { get; set; }

    }
}