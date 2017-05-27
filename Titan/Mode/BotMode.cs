using Serilog.Core;
using Titan.Logging;

namespace Titan.Mode
{

    public enum BotMode
    {

        Unknown = -1,
        Report = 0,
        Commend = 1

    }

    public class BotModeParser
    {

        private static Logger _log = LogCreator.Create();

        public static BotMode Parse(string mode)
        {
            switch(mode.ToUpperInvariant())
            {
               case "REPORT":
                   return BotMode.Report;
               case "COMMEND":
                   return BotMode.Commend;
               default:
                   _log.Error("Could not parse string to mode: {String}", mode);
                   break;
            }

            return BotMode.Unknown;
        }

    }

}