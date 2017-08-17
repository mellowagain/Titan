using System;

namespace Titan.Util
{
    public class RandomUtil
    {
        
        public static Random Random = new Random();

        public static uint RandomUInt32()
        {
            var array = new byte[4];
            Random.NextBytes(array);
            
            return BitConverter.ToUInt32(array, 0);
        }
        
    }
}