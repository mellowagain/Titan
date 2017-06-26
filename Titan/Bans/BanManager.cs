using System.Net;
using Serilog.Core;
using SteamKit2;
using Titan.Logging;
using Titan.Web;

namespace Titan.Bans
{
    public class BanManager
    {

        private Logger _log = LogCreator.Create();

        public BanInfo GetBanInfoFor(SteamID steamID)
        {
            try
            {
                using(dynamic steamUser = WebAPI.GetInterface("ISteamUser", WebAPIKeyResolver.APIKey))
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
            }
            catch (WebException ex)
            {
                _log.Error(ex, "A error occured when trying to get the Ban Information for {SteamID}.",
                    steamID.ConvertToUInt64());
            }

            _log.Warning("Did not receive ban informations for {SteamID}. Skipping...", steamID.ConvertToUInt64());
            return null;
        }

    }
}