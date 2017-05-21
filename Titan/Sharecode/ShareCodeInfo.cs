using Newtonsoft.Json;

namespace Titan.Sharecode
{
    public class ShareCodeInfo
    {

        [JsonProperty("matchID")]
        public ulong MatchID { get; set; }

    }
}