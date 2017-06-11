using System.Text.RegularExpressions;
using Serilog.Core;
using Titan.Logging;

namespace Titan.Mode
{

    public enum BotMode
    {

        Unknown = -1,
        Report = 0,
        Commend = 1,
        RemoveCommend = 2

    }

    public class BotModeParser
    {

        private static Logger _log = LogCreator.Create();

        public static BotMode Parse(string mode)
        {
            switch(Regex.Replace(mode.ToUpperInvariant(), @"\s+", ""))
            {
               case "REPORT":
                   return BotMode.Report;
               case "COMMEND":
                   return BotMode.Commend;
               case "UNCOMMEND":
               case "REMOVECOMMEND":
                   return BotMode.RemoveCommend;
               default:
                   _log.Error("Could not parse string to mode: {String}", mode);
                   break;
            }

            return BotMode.Unknown;
        }

    }

}