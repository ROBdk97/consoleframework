using System;

namespace ConsoleFramework
{
    public class TerminalSizeChangedEventArgs(int width, int height) : EventArgs
    {
        public readonly int Width = width;
        public readonly int Height = height;
    }
}

