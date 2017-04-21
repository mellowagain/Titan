using Eto.Forms;

namespace Titan.Core
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

    }
}