namespace ConsoleFramework.Native
{
    /// <summary>
    /// Structure to retrieve terminal size.
    /// </summary>
    public struct winsize
    {
        public ushort ws_row;
        public ushort ws_col;
        public ushort ws_xpixel;
        public ushort ws_ypixel;
    }
}
