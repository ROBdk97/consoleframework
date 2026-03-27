using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MOUSE_EVENT_RECORD
    {
        [FieldOffset(0)]
        public COORD dwMousePosition;
        [FieldOffset(4)]
        public MOUSE_BUTTON_STATE dwButtonState;
        [FieldOffset(8)]
        public ControlKeyState dwControlKeyState;
        [FieldOffset(12)]
        public MouseEventFlags dwEventFlags;
    }
}

