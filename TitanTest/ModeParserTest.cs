using Titan.Bot.Mode;
using Xunit;

namespace TitanTest
{
    public class ModeParserTest
    {

        [Theory]
        [InlineData("report")]
        [InlineData("commend")]
        public void TestParser(string s)
        {
            switch(s)
            {
                case "report":
                    Assert.True(BotModeParser.Parse(s) == BotMode.Report);
                    break;
                case "commend":
                    Assert.True(BotModeParser.Parse(s) == BotMode.Commend);
                    break;
            }

        }

    }
}