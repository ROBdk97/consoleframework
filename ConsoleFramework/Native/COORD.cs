using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD(short X, short Y)
    {
        public short X = X;
        public short Y = Y;
    };
}

