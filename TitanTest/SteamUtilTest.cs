using Titan.Util;
using Xunit;

namespace TitanTest
{
    public class SteamUtilTest
    {

        [Fact]
        public void TestSteamIDParser()
        {
            if(SteamUtil.FromSteamID("STEAM_0:0:131983088").ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Steam ID parsing successfull");
            }
            else
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void TestSteamID3Parser()
        {
            if(SteamUtil.FromSteamID3("[U:1:263966176]").ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Steam ID parsing successfull");
            }
            else
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void TestSteamID64Parser()
        {
            if(SteamUtil.FromSteamID64(76561198224231904).ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Steam ID parsing successfull");
            }
            else
            {
                Assert.True(false);
            }
        }

        [Theory]
        [InlineData("STEAM_0:0:131983088")]
        [InlineData("[U:1:263966176]")]
        [InlineData("76561198224231904")]
        public void TestAutoTypeParser(string id)
        {
            if(SteamUtil.Parse(id).ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Steam ID parsing successfull");
            }
            else
            {
                Assert.True(false);
            }
        }

    }
}