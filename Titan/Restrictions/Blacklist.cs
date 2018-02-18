using System.Collections.Generic;
using SteamKit2;

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

        public static List<string> BlackList = new List<string>
        {
            /* Titan Report & Commend Bot Admins */
            "STEAM_0:0:131983088", // Marc @ /id/marc3842h
            "STEAM_0:1:181127125", // Valentin @ /id/VLTN1337
            "STEAM_0:0:222174193", // Finn @ /id/anxyy
            "STEAM_0:1:162380002", // Lars @ /id/LmaoBucksSuck
            "STEAM_0:0:173511019", // Niko @ /profiles/76561198307287766
            "STEAM_0:0:211637257", // Jeremi @ /profiles/76561198383540242
            "STEAM_0:1:152354402", // Jakob @ /profiles/76561198264974533
            "STEAM_0:1:110191786", // Benjamin @ /id/rapsraps
            
            /* Titan Report & Commend Bot Alts of Admins */
            "STEAM_0:0:423939162", // Marc @ /id/traptsundere
            "STEAM_0:0:420897127", // Marc @ /profiles/76561198802059982
            "STEAM_0:0:209652364", // Valentin @ /profiles/76561198379570456
            "STEAM_0:0:207524819", // Valentin @ /id/lukesucksdick4free
            "STEAM_0:0:197017579", // Valentin @ /id/3438472347
            "STEAM_0:0:207321911", // Valentin @ /id/VLTNFAZE
            "STEAM_0:1:105570394", // Finn @ /id/Esel-Pfleger
            "STEAM_0:0:150730568", // Finn @ /id/CACAAAWWWWWWWWWWWWW
            "STEAM_0:0:208927995", // Lars @ /id/765619349858587
            "STEAM_0:0:197847637", // Niko @ /id/totallyNotNikosSmurf
            "STEAM_0:0:211686028", // Niko @ /profiles/76561198383637784
            
            /* Friends of Titan Report & Commend Bot */
            "STEAM_0:1:202915349", // LoiLock @ https://steamcommunity.com/id/LoiLock
            "STEAM_0:1:424753676", // Cheddar @ /profiles/76561198809773081
            "STEAM_0:0:170566473", // Eclip @ /profiles/76561198301398674
            "STEAM_0:1:74661837",  // Eclip @ /id/2789z
            "STEAM_0:0:85041889",  // Thunder @ /id/ThunderEOW 
            "STEAM_0:0:197847637"  // Tisjuboi @ /profiles/76561198808112544/
        };

        public static bool IsBlacklisted(SteamID steamID)
        {
            return !Titan.Instance.Options.DisableBlacklist && BlackList.Contains(steamID.Render(false));
        }
    
    }
}
