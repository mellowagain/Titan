using System.Linq;
using System.Net;
using Serilog.Core;
using SteamKit2;
using Titan.Logging;
using Titan.Web;

namespace Titan.Util
{
    public static class SteamUtil
    {

        private static Logger _log = LogCreator.Create();

        // Renders from a "STEAM_0:0:131983088" form.
        public static SteamID FromSteamID(string steamID)
        {
            return new SteamID(steamID);
        }

        // Renders from a "[U:1:263966176]" form.
        public static SteamID FromSteamID3(string steamID3)
        {
            var id = new SteamID();
            id.SetFromSteam3String(steamID3);

            return id;
        }

        // Renders from a "76561198224231904" form.
        public static SteamID FromSteamID64(ulong steamID64)
        {
            var id = new SteamID();
            id.SetFromUInt64(steamID64);

            return id;
        }
        
        // Renders from a "https://steamcommunity.com/id/Marc3842h/" form.
        public static SteamID FromCustomUrl(string customUrl)
        {
            var url = customUrl.StartsWith("http://") ? 
                customUrl.Replace("http://", "") : 
                customUrl.StartsWith("https://") ? 
                    customUrl.Replace("https://", "")
                    : customUrl;

            url = url.Replace("steamcommunity.com", "");

            url = url.Replace("/id/", "");

            url = url.Replace("/", "");

            try
            {
                using(dynamic steamUser = WebAPI.GetInterface("ISteamUser", WebAPIKeyResolver.APIKey))
                {    
                    KeyValue pair = steamUser.ResolveVanityURL(vanityurl: url);

                    if(pair["success"].AsInteger() == 1)
                    {
                        return FromSteamID64(pair["steamid"].AsUnsignedLong());
                    }
                    
                    _log.Error("Could not resolve custom URL {URL} to SteamID64: {Error}",
                        url, pair["message"].AsString());
                }
            }
            catch (WebException ex)
            {
                _log.Error("Could not resolve custom URL {URL} to SteamID64: {Error}",
                    url, ex.Message);
            }

            return null;
        }

        // Renders from a "http://steamcommunity.com/profiles/76561198224231904" form.
        public static SteamID FromNativeUrl(string nativeUrl)
        {
            var url = nativeUrl.StartsWith("http://") ? 
                nativeUrl.Replace("http://", "") : 
                nativeUrl.StartsWith("https://") ? 
                    nativeUrl.Replace("https://", "")
                    : nativeUrl;

            url = url.Replace("steamcommunity.com", "");

            url = url.Replace("/profiles/", "");
            
            url = url.Replace("/", "");

            return ulong.TryParse(url, out var steamID) ? FromSteamID64(steamID) : FromCustomUrl(nativeUrl);
        }

        public static SteamID Parse(string s)
        {
            switch(s.ElementAt(0))
            {
                    case '[':
                        return FromSteamID3(s);
                    case 'S':
                        return FromSteamID(s);
                    case 'h':
                        if(s.Contains("id"))
                        {
                            return FromCustomUrl(s);
                        }

                        return FromNativeUrl(s);
                    default:
                        ulong id;
                        return ulong.TryParse(s, out id) ? FromSteamID64(id) : FromCustomUrl(s);
            }
        }

    }
}