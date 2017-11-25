using Titan.MatchID.Sharecode;
using Xunit;

namespace TitanTest
{
    public class ShareCodeDecoderTest
    {

        [Fact]
        public void TestDecoder()
        {
            Assert.True(ShareCode.Decode("CSGO-727c4-5oCG3-PurVX-sJkdn-LsXfE").MatchID == 3208347562318757960);
        }

    }
}