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
        private string EnvironmentKey => Environment.GetEnvironmentVariable("TITAN_WEB_API_KEY");

        public BanManagerTest()
        {
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

            if (_handle.RequestBanInfo(SteamUtil.FromSteamID("STEAM_0:0:208017504"), out var banInfo))
            {
                Assert.True(banInfo.GameBanCount > 0);
            }
        }

        // STEAM_0:0:19877565
        // https://steamcommunity.com/id/kqly/
        [SkippableFact]
        public void TestVacBan()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");

            if (_handle.RequestBanInfo(SteamUtil.FromSteamID("STEAM_0:0:19877565"), out var banInfo))
            {
                Assert.True(banInfo.VacBanned);
            }
        }

        // STEAM_0:0:131983088
        // https://steamcommunity.com/id/Marc3842h/
        [SkippableFact]
        public void TestCleanHistory()
        {
            Skip.If(_handle.GetKey() == null, "No valid Steam Web API key has been provided with this test case.");

            if (_handle.RequestBanInfo(SteamUtil.FromSteamID("STEAM_0:0:131983088"), out var banInfo))
            {
                Assert.True(!banInfo.VacBanned && banInfo.GameBanCount <= 0);
            }
        }

    }
}
