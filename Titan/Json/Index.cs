using Newtonsoft.Json;

namespace Titan.Json
{
    public class Index
    {

        [JsonProperty("index")]
        public int AvailableIndex { get; set; } // Index that is available right now

        [JsonProperty("entries")]
        public IndexExpireEntry[] ExpireEntries { get; set; } // Entries that are marked for expiration

        public class IndexExpireEntry
        {

            [JsonProperty("index")]
            public int Index { get; set; } // Index that is marked for expiration

            [JsonProperty("expires")]
            public long Expires { get; set; } // UNIX timestamp when it expires

        }

    }
}