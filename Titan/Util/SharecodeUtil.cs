using Serilog.Core;
using Titan.Logging;
using Titan.Sharecode;

namespace Titan.Util
{
    public class SharecodeUtil
    {

        private static Logger _log = LogCreator.Create();

        public static ulong Parse(string shareCode)
        {
            if(shareCode.StartsWith("steam://"))
            {
                return new ShareCodeDecoder(shareCode.Substring(61)).Decode().MatchID;
            }

            if(shareCode.StartsWith("CSGO-"))
            {
                return new ShareCodeDecoder(shareCode).Decode().MatchID;
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