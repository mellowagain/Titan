using System;
using System.IO;
using Titan.Util;
using Titan.Web;
using Xunit;

namespace TitanTest
{
    public class BanManagerTest
    {

        private SWAHandle _handle = new SWAHandle();
        private string EnvironmentKey => Environment.GetEnvironmentVariable(
            "TITAN_WEB_API_KEY", EnvironmentVariableTarget.User
        );

        public BanManagerTest()
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

        // STEAM_0:0:208017504
        // https://steamcommunity.com/id/TopKekTux/
        [SkippableFact]
        public void TestGameBan()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");

            var banInfo = _handle.RequestBanInfo(SteamUtil.FromSteamID("STEAM_0:0:208017504"));

            if(banInfo != null && banInfo.GameBanCount > 0)
            {
                Assert.True(true, "topkektux has game bans on record.");
            }
            else
            {
                Assert.True(false);
            }
        }

        // STEAM_0:0:19877565
        // https://steamcommunity.com/id/kqly/
        [SkippableFact]
        public void TestVacBan()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");
            
            var banInfo = _handle.RequestBanInfo(SteamUtil.FromSteamID("STEAM_0:0:19877565"));

            if(banInfo != null && banInfo.VacBanned)
            {
                Assert.True(true, "KQLY has VAC bans on record.");
            }
            else
            {
                Assert.True(false);
            }
        }

        // STEAM_0:0:131983088
        // https://steamcommunity.com/id/Marc3842h/
        [SkippableFact]
        public void TestCleanHistory()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");
            
            var banInfo = _handle.RequestBanInfo(SteamUtil.FromSteamID("STEAM_0:0:131983088"));

            if(banInfo != null && !banInfo.VacBanned && banInfo.GameBanCount <= 0)
            {
                Assert.True(true, "Marc3842h has no bans on record.");
            }
            else
            {
                Assert.True(false);
            }
        }

    }
}