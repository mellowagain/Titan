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

        public SteamUtilTest()
        {
            // Workaround for Mono related issue regarding System.Net.Http.
            // More detail: https://github.com/dotnet/corefx/issues/19914

            var systemNetHttpDll = new FileInfo(Path.Combine(Environment.CurrentDirectory, "System.Net.Http.dll"));
            
            if (systemNetHttpDll.Exists && !PlatformUtil.IsWindows())
            {
                systemNetHttpDll.Delete();
            }
            
            WebAPIKeyResolver.APIKey = Environment.GetEnvironmentVariable("TITAN_WEB_API_KEY");
        }

        [Fact]
        public void TestSteamIDParser()
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                if(FromSteamID("STEAM_0:0:131983088").ConvertToUInt64() == 76561198224231904)
                {
                    Assert.True(true, "Steam ID parsing successfull");
                }
                else
                {
                    Assert.True(false);
                }
            }
        }

        [Fact]
        public void TestSteamID3Parser()
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                if(FromSteamID3("[U:1:263966176]").ConvertToUInt64() == 76561198224231904)
                {
                    Assert.True(true, "Steam ID parsing successfull");
                }
                else
                {
                    Assert.True(false);
                }
            }
        }

        [Fact]
        public void TestSteamID64Parser()
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                if(FromSteamID64(76561198224231904).ConvertToUInt64() == 76561198224231904)
                {
                    Assert.True(true, "Steam ID parsing successfull");
                }
                else
                {
                    Assert.True(false);
                }
            }
        }

        [Fact]
        public void TestCustomURLParser()
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                if(FromCustomUrl("https://steamcommunity.com/id/Marc3842h/").ConvertToUInt64() == 76561198224231904)
                {
                    Assert.True(true, "Steam ID parsing successfull");
                }
                else
                {
                    Assert.True(false);
                }
            }
        }

        [Fact]
        public void TestNativeURLParser()
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                if(FromNativeUrl("http://steamcommunity.com/profiles/76561198224231904").ConvertToUInt64() ==
                   76561198224231904)
                {
                    Assert.True(true, "Steam ID parsing successfull");
                }
                else
                {
                    Assert.True(false);
                }
            }
        }

        [Theory]
        [InlineData("STEAM_0:0:131983088")]
        [InlineData("[U:1:263966176]")]
        [InlineData("76561198224231904")]
        [InlineData("https://steamcommunity.com/id/Marc3842h/")]
        [InlineData("http://steamcommunity.com/profiles/76561198224231904")]
        public void TestAutoTypeParser(string id)
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                if(Parse(id).ConvertToUInt64() == 76561198224231904)
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
}