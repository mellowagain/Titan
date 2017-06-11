using Titan.Mode;
using Xunit;

namespace TitanTest
{
    public class ModeParserTest
    {

        [Theory]
        [InlineData("report")]
        [InlineData("commend")]
        [InlineData("uncommend")]
        [InlineData("remove commend")]
        [InlineData("unknown")] // This bot mode doesn't exist, so it will return Unknown.
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
                case "uncommend":
                case "remove commend":
                    Assert.True(BotModeParser.Parse(s) == BotMode.RemoveCommend);
                    break;
                case "unknown":
                    Assert.True(BotModeParser.Parse(s) == BotMode.Unknown);
                    break;
            }

        }

    }
}