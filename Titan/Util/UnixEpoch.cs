using System;

namespace Titan.Util
{
    public static class UnixEpoch
    {

        public static long ToEpochTime(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }
        
        public static DateTime ToDateTime(this long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }
        
    }
}
