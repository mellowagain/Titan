using Newtonsoft.Json;

namespace Titan.Json
{
    public class JsonIndex
    {

        [JsonProperty("index")]
        public int AvailableIndex;

        [JsonProperty("entries")]
        public JsonEntry[] Entries;

        public class JsonEntry
        {

            [JsonProperty("index")]
            public int TargetedIndex;

            [JsonProperty("expires")]
            public double ExpireTimestamp;

        }

    }
}