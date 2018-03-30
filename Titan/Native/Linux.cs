using System.Runtime.InteropServices;

namespace Titan.Native
{
    public static class Linux
    {
        #if __UNIX__

        [DllImport("libc", SetLastError = true)]
        public static extern uint getuid();

        #endif
    }
}
