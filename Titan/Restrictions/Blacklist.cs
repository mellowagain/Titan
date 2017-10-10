using System.Collections.Generic;
using SteamKit2;
using Titan.Util;

namespace Titan.Restrictions
{
    public static class Blacklist
    {
        
        // This is the hardcoded implementation of a Blacklist in Titan.
        // This prevents users from botting persons on this
        // blacklist, to prevent my own accounts and thse
        // from my friends from getting report botted and banned.
        
        // Purchasing yourself into this blacklist is not possible
        // and this whole application is open-source. If users
        // wish to disable the blacklist, they could either remove
        // the whole blacklist or include the "--noblacklist" command
        // line argument when starting Titan.

        public static List<string> BlackList = new List<string>(new[]
        {
            "STEAM_0:0:131983088", // /id/marc3842h -                     Marc
            "STEAM_0:1:169234413", // /id/marc3842s -                     Marc
            "STEAM_0:1:226504294", // /id/lolokweeb -                     Marc
            "STEAM_0:0:173511019", // /id/nikothebeasT -                  Niko
            "STEAM_0:0:197847637", // /id/totallyNotNikosSmurf -          Niko
            "STEAM_0:1:181127125", // /id/VLTN1337 -                      Valentin
            "STEAM_0:0:197017579", // /id/3438472347 -                    Valentin
            "STEAM_0:0:207321911", // /id/VLTNFAZE -                      Valentin
            "STEAM_0:0:207524819", // /id/lukesucksdick4free -            Valentin
            "STEAM_0:0:209652364", // /profiles/76561198379570456 -       Valentin
            "STEAM_0:1:162380002", // /profiles/76561198285025733 -       Lars
            "STEAM_0:0:208927995", // /id/765619349858587 -               Lars
            "STEAM_0:1:118031819", // /profiles/76561198196329367 -       Jerri
            "STEAM_0:0:211637257", // /profiles/76561198383540242 -       Jerri
            "STEAM_0:1:211200114", // /profiles/76561198382665957 -       Jerri
            "STEAM_0:1:115536175", // /profiles/76561198191338079 -       Finn
            "STEAM_0:0:150730568", // /id/ulber123 -                      Finn
            "STEAM_0:1:152354402", // /profiles/76561198264974533 -       Jakob
            "STEAM_0:1:110468204", // /id/urlvonmir -                     Paul
            "STEAM_0:0:231438702", // /id/isaweeb/ -                      Benjamin
            "STEAM_0:1:216831063", // /profiles/76561198393927855 -       Ichi
        });

        public static bool IsBlacklisted(SteamID steamID)
        {
            return !Titan.Instance.Options.DisableBlacklist && BlackList.Contains(steamID.Render());
        }
    
    }
}