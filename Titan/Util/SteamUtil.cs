using System.Linq;
using Serilog;
using SteamKit2;
using Titan.Logging;

namespace Titan.Util
{
    public class SteamUtil
    {

        private static ILogger _log = LogCreator.Create();

        // Renders from a "STEAM_0:0:131983088" form.
        public static SteamID FromSteamID(string steamId)
        {
            return new SteamID(steamId);
        }

        // Renders from a "[U:1:263966176]" form.
        public static SteamID FromSteamID3(string steamId3)
        {
            var id = new SteamID();
            id.SetFromSteam3String(steamId3);

            return id;
        }

        // Renders from a "76561198224231904" form.
        public static SteamID FromSteamID64(ulong steamId64)
        {
            var id = new SteamID();
            id.SetFromUInt64(steamId64);

            return id;
        }

        public static SteamID Parse(string s)
        {
            switch(s.ElementAt(0))
            {
                    case '[':
                        return FromSteamID3(s);
                    case 'S':
                        return FromSteamID(s);
                    default:
                        ulong id;
                        if(ulong.TryParse(s, out id))
                        {
                            return FromSteamID64(id);
                        }
                        
                        _log.Error("Could not parse {SteamID} to Steam ID.", s);
                        return null; // TODO: Handle errored Steam ID better.
            }
        }

    }
}