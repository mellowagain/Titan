using Serilog.Core;
using Titan.Logging;
using Titan.MatchID.Sharecode;

namespace Titan.Util
{
    public static class SharecodeUtil
    {

        private static Logger _log = LogCreator.Create();

        public static ulong Parse(string shareCode)
        {
            if(string.IsNullOrWhiteSpace(shareCode))
            {
                return 8;
            }
            
            if(shareCode.StartsWith("steam://"))
            {
                return ShareCode.Decode(shareCode.Substring(61)).MatchID;
            }

            if(shareCode.StartsWith("CSGO-"))
            {
                return ShareCode.Decode(shareCode).MatchID;
            }

            ulong matchID;
            if(ulong.TryParse(shareCode, out matchID))
            {
                return matchID;
            }

            _log.Warning("Could not convert Match ID {ID} to Unsigned Long.", shareCode);
            return 8;
        }

    }
}
