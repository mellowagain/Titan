using System;
using System.IO;
using Titan.Util;
using Titan.Web;
using Xunit;
using static Titan.Util.SteamUtil;

namespace TitanTest
{
    public class SteamUtilTest
    {
        
        private SWAHandle _handle = new SWAHandle();
        private string EnvironmentKey => Environment.GetEnvironmentVariable("TITAN_WEB_API_KEY");

        public SteamUtilTest()
        {
            // Workaround for Mono related issue regarding System.Net.Http.
            // More detail: https://github.com/dotnet/corefx/issues/19914

            var systemNetHttpDll = new FileInfo(Path.Combine(Environment.CurrentDirectory, "System.Net.Http.dll"));
            
            if (systemNetHttpDll.Exists && !PlatformUtil.IsWindows())
            {
                systemNetHttpDll.Delete();
            }
            
            if (!string.IsNullOrEmpty(EnvironmentKey))
            {
                _handle.SetKey(EnvironmentKey);
            }
        }

        [SkippableFact]
        public void TestSteamIDParser()
        {
            Assert.True(FromSteamID("STEAM_0:0:131983088").ConvertToUInt64() == 76561198224231904);
        }

        [SkippableFact]
        public void TestSteamID3Parser()
        {
            Assert.True(FromSteamID3("[U:1:263966176]").ConvertToUInt64() == 76561198224231904);
        }

        [SkippableFact]
        public void TestSteamID64Parser()
        {
            Assert.True(FromSteamID64(76561198224231904).ConvertToUInt64() == 76561198224231904);
        }

        [SkippableFact]
        public void TestCustomURLParser()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");
            
            Assert.True(FromCustomUrl("https://steamcommunity.com/id/Marc3842h/", _handle).ConvertToUInt64() == 
                        76561198224231904);
        }

        [SkippableFact]
        public void TestNativeURLParser()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");
            
            Assert.True(FromNativeUrl(
                            "http://steamcommunity.com/profiles/76561198224231904", _handle
                        ).ConvertToUInt64() == 76561198224231904);
        }

        [SkippableTheory]
        [InlineData("STEAM_0:0:131983088")]
        [InlineData("[U:1:263966176]")]
        [InlineData("76561198224231904")]
        [InlineData("https://steamcommunity.com/id/Marc3842h/")]
        [InlineData("http://steamcommunity.com/profiles/76561198224231904")]
        public void TestAutoTypeParser(string id)
        {
            Skip.If(_handle.GetKey() == null && id.StartsWith("h"), 
                    "No valid Steam Web API key has been provided with this test case.");
            
            Assert.True(Parse(id, _handle).ConvertToUInt64() == 76561198224231904);
        }

    }
}