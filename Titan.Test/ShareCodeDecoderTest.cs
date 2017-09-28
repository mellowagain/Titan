using Titan.MatchID.Sharecode;
using Xunit;

namespace Titan.Test
{
    public class ShareCodeDecoderTest
    {

        [Fact]
        public void TestDecoder()
        {
            if(ShareCode.Decode("CSGO-727c4-5oCG3-PurVX-sJkdn-LsXfE").MatchID == 3208347562318757960)
            {
                Assert.True(true, "The decoded Match ID is 3208347562318757960");
            }
            else
            {
                Assert.True(false);
            }
        }

    }
}