using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_CURSOR_INFO
    {
        /// <summary>
        /// The percentage of the character cell that is filled by the cursor.
        /// This value is between 1 and 100. The cursor appearance varies, ranging from completely
        /// filling the cell to showing up as a horizontal line at the bottom of the cell.
        /// </summary>
        public uint Size;
        public bool Visible;
    }
}

