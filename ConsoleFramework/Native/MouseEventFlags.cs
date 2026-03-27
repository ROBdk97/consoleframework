using System;

namespace ConsoleFramework.Native
{
    [Flags]
    public enum MouseEventFlags
    {
        PRESSED_OR_RELEASED = 0x0000,
        DOUBLE_CLICK = 0x0002,
        MOUSE_HWHEELED = 0x0008,
        MOUSE_MOVED = 0x0001,
        MOUSE_WHEELED = 0x0004
    }
}

