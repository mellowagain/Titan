using Eto.Forms;

namespace Titan.Bot.Mode
{
    public class ModeParser
    {

        public static BotMode Parse(string mode)
        {
            switch(mode.ToLower())
            {
                case "commend":
                    return BotMode.Commend;
                case "report":
                    return BotMode.Report;
                default:
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