using System.Linq;
using SteamKit2;

namespace Titan.Util
{
    public class SteamUtil
    {

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
                        return FromSteamID64(ulong.Parse(s));
            }
        }

    }
}