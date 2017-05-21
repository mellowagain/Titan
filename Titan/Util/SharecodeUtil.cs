using System;
using Titan.Sharecode;

namespace Titan.Util
{
    public class SharecodeUtil
    {

        public static ulong Parse(string shareCode)
        {
            if(shareCode.StartsWith("steam://"))
                return new ShareCodeDecoder(shareCode.Substring(61)).Decode().MatchID;

            return shareCode.StartsWith("CSGO-") ? new ShareCodeDecoder(shareCode).Decode().MatchID : Convert.ToUInt64(shareCode);
        }

    }
}