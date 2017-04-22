using Eto.Forms;
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
                    _log.Information("Could not parse {Mode} to BotMode. Please change it to either \"Commend\" or \"Report\".",
                        mode);
                    MessageBox.Show("Could not parse \"" + mode + "\" to BotMode. " +
                                    "Please change it to either \"Commend\" or \"Report\".",
                        "Titan - Error", MessageBoxType.Error);
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
                    MessageBox.Show("Could not parse \"" + index + "\" to BotMode. " +
                                    "Please change it to either \"1\" or \"0\".",
                        "Titan - Error", MessageBoxType.Error);
                    return BotMode.Report;
            }
        }

    }
}