using System;
using Eto.Forms;
using SteamKit2.GC.TF2.Internal;
using Titan.Account;

namespace Titan.Util
{
    public static class GameUtil
    {
        
        public static uint ToAppID(this DropDown drop)
        {
            if (drop.Items.Count != 2)
                throw new ArgumentException("Only Game Drop Downs can be converted to a App ID.");

            switch (drop.SelectedIndex)
            {
                case 0:
                    return TitanAccount.CSGO_APPID;
                case 1:
                    return TitanAccount.TF2_APPID;
                default:
                    return 0;
            }
        }

        public static uint ToAppID(this string input)
        {
            switch (input.ToLower().Trim())
            {
                case "csgo":
                    return TitanAccount.CSGO_APPID;
                case "tf2":
                    return TitanAccount.TF2_APPID;
                default:
                    throw new InvalidCastException("Cant cast " + input + " to valid app id.");
            }
        }
        
        // TF2
        public static CMsgGC_ReportPlayer.EReason ToTF2ReportReason(this DropDown drop)
        {
            if (drop.Items.Count != 4)
                throw new ArgumentException("Only TF2 Report Reason Drop Downs can be converted to report reason.");

            switch (drop.SelectedIndex)
            {
                case 0:
                    return CMsgGC_ReportPlayer.EReason.kReason_CHEATING;
                case 1:
                    return CMsgGC_ReportPlayer.EReason.kReason_IDLE;
                case 2:
                    return CMsgGC_ReportPlayer.EReason.kReason_HARASSMENT;
                case 3:
                    return CMsgGC_ReportPlayer.EReason.kReason_GRIEFING;
            }

            return CMsgGC_ReportPlayer.EReason.kReason_INVALID;
        }
        
    }
}
