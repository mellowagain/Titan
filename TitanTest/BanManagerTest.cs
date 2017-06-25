using System;
using Titan.Bans;
using Titan.Util;
using Titan.Web;
using Xunit;

namespace TitanTest
{
    public class BanManagerTest
    {

        private BanManager _banManager = new BanManager();

        public BanManagerTest()
        {
            WebAPIKeyResolver.APIKey = Environment.GetEnvironmentVariable("TITAN_WEB_API_KEY");
        }

        [Fact]
        public void TestGameBan()
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                var banInfo = _banManager.GetBanInfoFor(SteamUtil.FromSteamID("STEAM_0:0:208017504"));

                if(banInfo != null && banInfo.GameBanCount > 0)
                {
                    Assert.True(true, "topkektux has game bans on record.");
                }
                else
                {
                    Assert.True(false);
                }
                // STEAM_0:0:208017504
                // https://steamcommunity.com/id/TopKekTux/
            }
        }

        [Fact]
        public void TestVacBan()
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                var banInfo = _banManager.GetBanInfoFor(SteamUtil.FromSteamID("STEAM_0:0:19877565"));

                if(banInfo != null && banInfo.VacBanned)
                {
                    Assert.True(true, "KQLY has VAC bans on record.");
                }
                else
                {
                    Assert.True(false);
                }
                // STEAM_0:0:19877565
                // https://steamcommunity.com/id/kqly/
            }
        }

        [Fact]
        public void TestCleanHistory()
        {
            if(WebAPIKeyResolver.APIKey != null)
            {
                var banInfo = _banManager.GetBanInfoFor(SteamUtil.FromSteamID("STEAM_0:0:131983088"));

                if(banInfo != null && (!banInfo.VacBanned && banInfo.GameBanCount <= 0))
                {
                    Assert.True(true, "Marc3842h has no bans on record.");
                }
                else
                {
                    Assert.True(false);
                }
                // STEAM_0:0:131983088
                // https://steamcommunity.com/id/Marc3842h/
            }
        }

    }
}