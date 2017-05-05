using SteamKit2;
using Titan.Bot.Bans;
using Xunit;

namespace TitanTest
{
    public class BanManagerTest
    {

        // README: You need to place a file with a SteamAPI Web key in the bin/debug folder of the
        // TitanTest directory named "steamapi.key" for this to pass.

        private BanManager _banManager = new BanManager();

        public BanManagerTest()
        {
            _banManager.ParseApiKeyFile();
        }

        [Fact]
        public void TestGameBan()
        {
            var banInfo = _banManager.GetBanInfoFor(new SteamID("[U:1:416035008]").ConvertToUInt64());

            if(banInfo != null && banInfo.GameBanCount > 0)
            {
                Assert.True(true, "topkektux has game bans on record.");
            }
            else
            {
                Assert.True(false);
            }
            // [U:1:416035008]
            // https://steamcommunity.com/id/TopKekTux/
        }

        [Fact]
        public void TestVacBan()
        {
            var banInfo = _banManager.GetBanInfoFor(new SteamID("[U:1:39755130]").ConvertToUInt64());

            if(banInfo != null && banInfo.VacBanned)
            {
                Assert.True(true, "KQLY has VAC bans on record.");
            }
            else
            {
                Assert.True(false);
            }
            // [U:1:39755130]
            // https://steamcommunity.com/id/kqly/
        }

        [Fact]
        public void TestCleanHistory()
        {
            var banInfo = _banManager.GetBanInfoFor(new SteamID("[U:1:263966176]").ConvertToUInt64());

            if(banInfo != null && (!banInfo.VacBanned && banInfo.GameBanCount <= 0))
            {
                Assert.True(true, "Marc3842h has no bans on record.");
            }
            else
            {
                Assert.True(false);
            }
            // [U:1:263966176]
            // https://steamcommunity.com/id/Marc3842h/
        }

    }
}