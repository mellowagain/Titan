using Newtonsoft.Json;

namespace Titan.Json
{
    public class Accounts
    {

        [JsonProperty("accounts")]
        public JsonAccount[] JsonAccounts { get; set; }

        public class JsonAccount
        {

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

        }

    }
}