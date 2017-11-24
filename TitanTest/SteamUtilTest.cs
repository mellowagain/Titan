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
        private string EnvironmentKey => Environment.GetEnvironmentVariable(
            "TITAN_WEB_API_KEY", EnvironmentVariableTarget.User
        );

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
            if(FromSteamID("STEAM_0:0:131983088").ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Output Steam ID is valid");
            }
            else
            {
                Assert.True(false);
            }
        }

        [SkippableFact]
        public void TestSteamID3Parser()
        {
            if(FromSteamID3("[U:1:263966176]").ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Output Steam ID is valid");
            }
            else
            {
                Assert.True(false);
            }
        }

        [SkippableFact]
        public void TestSteamID64Parser()
        {
            if(FromSteamID64(76561198224231904).ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Output Steam ID is valid");
            }
            else
            {
                Assert.True(false);
            }
        }

        [SkippableFact]
        public void TestCustomURLParser()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");
            
            if(FromCustomUrl("https://steamcommunity.com/id/Marc3842h/").ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Output Steam ID is valid");
            }
            else
            {
                Assert.True(false);
            }
        }

        [SkippableFact]
        public void TestNativeURLParser()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");
            
            if(FromNativeUrl("http://steamcommunity.com/profiles/76561198224231904").ConvertToUInt64() ==
               76561198224231904)
            {
                Assert.True(true, "Output Steam ID is valid");
            }
            else
            {
                Assert.True(false);
            }
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
            
            if(Parse(id).ConvertToUInt64() == 76561198224231904)
            {
                Assert.True(true, "Output Steam ID is valid");
            }
            else
            {
                Assert.True(false);
            }
        }

    }
}