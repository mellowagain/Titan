using Newtonsoft.Json;

namespace Titan.Json
{
    public class JsonAccounts
    {

        [JsonProperty("indexes")]
        public JsonIndex[] Indexes { get; set; }

        public class JsonIndex
        {

            [JsonProperty("accounts")]
            public JsonAccount[] Accounts { get; set; }

        }

        public class JsonAccount
        {

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

            [JsonProperty("sentry")]
            public bool Sentry { get; set; }

            [JsonProperty("enabled")]
            public bool Enabled { get; set; }

        }

    }
}