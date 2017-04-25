using Serilog.Core;
using Titan.Logging;

namespace Titan.Bot.Mode
{
    public class ModeParser
    {

        private static Logger _log = LogCreator.Create();

        public static BotMode Parse(string mode)
        {
            switch(mode.ToLower())
            {
                case "commend":
                    return BotMode.Commend;
                case "report":
                    return BotMode.Report;
                default:
                    _log.Error("Could not parse {Mode} to BotMode. Please change it to " +
                               "either \"Commend\" or \"Report\".",
                        mode);
                    return BotMode.Report;
            }
        }

        public static BotMode Parse(int index)
        {
            switch(index)
            {
                case 1:
                    return BotMode.Commend;
                case 0:
                    return BotMode.Report;
                default:
                    _log.Error("Could not parse {Index} to BotMode. Please change it to either " +
                               "0 for Report or 1 for Commend.");
                    return BotMode.Report;
            }
        }

    }
}