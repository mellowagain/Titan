using System.ComponentModel;
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

            [JsonProperty("username", Required = Required.DisallowNull)]
            public string Username { get; set; }

            [JsonProperty("password", Required = Required.DisallowNull)]
            public string Password { get; set; }

            [JsonProperty("sentry", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            [DefaultValue(false)]
            public bool Sentry { get; set; }

            [JsonProperty("enabled", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            [DefaultValue(true)]
            public bool Enabled { get; set; }
            
            [JsonProperty("secret", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            [DefaultValue(null)]
            public string SharedSecret { get; set; }

        }

    }
}