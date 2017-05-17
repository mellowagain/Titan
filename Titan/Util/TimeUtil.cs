using System;

namespace Titan.Util
{
    public class TimeUtil
    {

        public static long GetCurrentTicks()
        {
            return DateTimeToTicks(DateTime.Now);
        }

        public static long DateTimeToTicks(DateTime time)
        {
            return time.Ticks;
        }

        public static DateTime TicksToDateTime(long ticks)
        {
            return new DateTime(ticks);
        }

    }
}