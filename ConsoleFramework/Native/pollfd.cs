using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct pollfd
    {
        public int fd;
        public POLL_EVENTS events;
        public POLL_EVENTS revents;
    }
}
