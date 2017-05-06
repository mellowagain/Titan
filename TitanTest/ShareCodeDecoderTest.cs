using System;
using Titan.Sharecode;
using Xunit;

namespace TitanTest
{
    public class ShareCodeDecoderTest
    {

        [Fact]
        public void TestDecoder()
        {
            var decoder = new ShareCodeDecoder("CSGO-727c4-5oCG3-PurVX-sJkdn-LsXfE");

            if(decoder.Decode().MatchId == 3208347562318757960)
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