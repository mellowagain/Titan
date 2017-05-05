using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using SteamKit2;
using Titan.Logging;

namespace Titan.Bot.Bans
{
    public class BanManager
    {

        private Logger _log = LogCreator.Create();

        private FileInfo _apiKeyFile;

        private string _apiKey;

        public BanManager()
        {
            _apiKeyFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, "steamapi.key"));
        }

        public void ParseApiKeyFile()
        {
            if(!_apiKeyFile.Exists)
            {
                _log.Information("======================");
                _log.Information("The {Reason} key has not been found.", "Steam API");
                _log.Information("Please generate a API key at {Url} " +
                                 "and input it below:", "https://steamcommunity.com/dev/apikey");
                _log.Information("======================");
                _apiKey = Console.ReadLine();

                File.WriteAllText(_apiKeyFile.ToString(), _apiKey);

                // TODO: Change this into a form
            }

            using(var reader = File.OpenText(_apiKeyFile.ToString()))
            {
                _apiKey = reader.ReadLine();
            }

            _log.Debug("Using Steam API key: {Key}", _apiKey);
        }

        public BanInfo GetBanInfoFor(ulong steamId)
        {
            using(dynamic steamUser = WebAPI.GetInterface("ISteamUser", _apiKey))
            {
                KeyValue pair = steamUser.GetPlayerBans(steamids: steamId);

                foreach(var get in pair["players"].Children)
                {
                    if(get["SteamId"].AsUnsignedLong() == steamId)
                    {
                        return new BanInfo
                        {
                            SteamId = get["SteamId"].AsUnsignedLong(),
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

            return null;
        }

    }
}