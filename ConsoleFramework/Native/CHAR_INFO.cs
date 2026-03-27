using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{
    /// <summary>
    /// CharSet.Unicode is required for proper marshaling.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CHAR_INFO
    {
        [FieldOffset(0)]
        public char UnicodeChar;
        [FieldOffset(0)]
        public char AsciiChar;
        [FieldOffset(2)] //2 bytes seems to work properly
        public Attr Attributes;

        public override string ToString()
        {
            return string.Format("CHAR_INFO : '{0}' ({1})", AsciiChar, Attributes);
        }
    }
}

