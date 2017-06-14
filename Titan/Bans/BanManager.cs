using System;
using System.IO;
using System.Text;
using Serilog.Core;
using SteamKit2;
using Titan.Logging;
using Titan.UI;

namespace Titan.Bans
{
    public class BanManager
    {

        private Logger _log = LogCreator.Create();

        private FileInfo _apiKeyFile;

        public string APIKey;

        public BanManager()
        {
            _apiKeyFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, "steamapi.key"));
        }

        public void ParseApiKeyFile()
        {
            if(!_apiKeyFile.Exists)
            {
                Titan.Instance.UIManager.ShowForm(UIType.APIKeyInput);
            }
            else
            {
                using(var reader = File.OpenText(_apiKeyFile.ToString()))
                {
                    APIKey = reader.ReadLine();
                }

                if(string.IsNullOrWhiteSpace(APIKey))
                {
                    APIKey = null;
                    Titan.Instance.UIManager.ShowForm(UIType.APIKeyInput);
                }
            }

            _log.Debug("Using Steam API key: {Key}", APIKey);
        }

        public void SaveAPIKeyFile()
        {
            Environment.SetEnvironmentVariable("TITAN_WEB_API_KEY", APIKey, EnvironmentVariableTarget.User);

            File.WriteAllText(_apiKeyFile.ToString(), APIKey, Encoding.UTF8);
        }

        public BanInfo GetBanInfoFor(SteamID steamID)
        {
            using(dynamic steamUser = WebAPI.GetInterface("ISteamUser", APIKey))
            {
                KeyValue pair = steamUser.GetPlayerBans(steamids: steamID.ConvertToUInt64());

                foreach(var get in pair["players"].Children)
                {
                    if(get["SteamId"].AsUnsignedLong() == steamID.ConvertToUInt64())
                    {
                        return new BanInfo
                        {
                            SteamID = get["SteamId"].AsUnsignedLong(),
                            CommunityBanned = get["CommunityBanned"].AsBoolean(),
                            VacBanned = get["VACBanned"].AsBoolean(),
                            VacBanCount = get["NumberOfVACBans"].AsInteger(),
                            DaysSinceLastBan = get["DaysSinceLastBan"].AsInteger(),
                            GameBanCount = get["NumberOfGameBans"].AsInteger(),
                            EconomyBan = get["EconomyBan"].AsString()
                        };
                    }
                }
            }

            _log.Warning("Did not receive ban informations for {SteamID}. Skipping...");
            return null;
        }

    }
}