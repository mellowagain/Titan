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
                    Assert.True(ModeParser.Parse(s) == BotMode.Report);
                    break;
                case "commend":
                    Assert.True(ModeParser.Parse(s) == BotMode.Commend);
                    break;
            }

        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void TestParser(int i)
        {
            switch(i)
            {
                case 0:
                    Assert.True(ModeParser.Parse(i) == BotMode.Report);
                    break;
                case 1:
                    Assert.True(ModeParser.Parse(i) == BotMode.Commend);
                    break;
            }
        }

    }
}