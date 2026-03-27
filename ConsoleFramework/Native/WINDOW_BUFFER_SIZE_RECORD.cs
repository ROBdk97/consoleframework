namespace ConsoleFramework.Native
{
    public struct WINDOW_BUFFER_SIZE_RECORD(short x, short y)
    {
        public COORD dwSize = new COORD(x, y);
    }
}

