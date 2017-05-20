using System;
using Titan.Bot.Mode;
using Titan.Util;

namespace Titan.Bot.Account
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

            // TODO: Replace with sharecode parsing as soon as done
        }

    }
}