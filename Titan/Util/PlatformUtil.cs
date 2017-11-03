using System;

namespace Titan.Util
{
    public static class PlatformUtil
    {
        
        private static int _platform;
        
        static PlatformUtil()
        {
            _platform = (int) Environment.OSVersion.Platform;
        }

        public static bool IsWindows()
        {
            return !IsLinux() && !IsMacOSX();
        }
        
        public static bool IsMacOSX()
        {
            return _platform == 6;
        }
        
        public static bool IsLinux()
        {
            return _platform == 4 || _platform == 128;
        }
        
    }
}