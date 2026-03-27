using System;

namespace ConsoleFramework.Native
{
    [Flags]
    public enum ControlKeyState
    {
        CAPSLOCK_ON = 0x0080,
        ENHANCED_KEY = 0x0100,
        LEFT_ALT_PRESSED = 0x0002,
        LEFT_CTRL_PRESSED = 0x0008,
        NUMLOCK_ON = 0x0020,
        RIGHT_ALT_PRESSED = 0x0001,
        RIGHT_CTRL_PRESSED = 0x0004,
        SCROLLLOCK_ON = 0x0040,
        SHIFT_PRESSED = 0x0010
    }
}

