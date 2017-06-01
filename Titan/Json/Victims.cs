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
            public double Ticks; // Timestamp in ticks that the target was botted

        }
        
    }
}