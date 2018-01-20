using Newtonsoft.Json;

namespace Titan.Json
{
    public class Victims
    {

        [JsonProperty("victims")] 
        public Victim[] Array;

        public class Victim
        {

            [JsonProperty("steamid")]
            public ulong SteamID; // Steam64ID of target

            [JsonProperty("timestamp")]
            public long Timestamp; // UNIX Epoch Time timestamp when the target was botted

        }
        
    }
}