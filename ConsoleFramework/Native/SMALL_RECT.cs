using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SMALL_RECT(short left, short top, short right, short bottom)
    {
        public short Left = left;
        public short Top = top;
        public short Right = right;
        public short Bottom = bottom;
    }
}

