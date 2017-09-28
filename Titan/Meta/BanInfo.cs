namespace Titan.Meta
{
    public class BanInfo
    {

        public ulong SteamID { get; set; }

        public bool CommunityBanned { get; set; }

        public bool VacBanned { get; set; }

        public int VacBanCount { get; set; }

        public int DaysSinceLastBan { get; set; }

        public int GameBanCount { get; set; }

        public string EconomyBan { get; set; }

    }
}