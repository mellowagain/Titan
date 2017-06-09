using System;
using Titan.Mode;
using Titan.Util;

namespace Titan.Account
{
    public class Info
    {

        public uint Target { get; set; }
        public ulong MatchID { get; set; }
        public BotMode Mode { get; set; }

        public void FeedWithTarget(string target)
        {
            Target = SteamUtil.Parse(target).AccountID;
        }

        public void FeedWithMatchID(string matchID)
        {
            MatchID = Convert.ToUInt64(matchID);
        }

    }
}