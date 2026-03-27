using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{

    // why sizeof it is 8 ?
    [StructLayout(LayoutKind.Explicit)]
    public struct code
    {
        // NOT long ! actually int
        [FieldOffset(0)] public int codepoint; /* TERMKEY_TYPE_UNICODE */
        [FieldOffset(0)] public int number; /* TERMKEY_TYPE_FUNCTION */
        [FieldOffset(0)] public TermKeySym sym; /* TERMKEY_TYPE_KEYSYM */
        [FieldOffset(0)] public byte mouse0; /* TERMKEY_TYPE_MOUSE (char[4]) */
        [FieldOffset(1)] public byte mouse1;
        [FieldOffset(2)] public byte mouse2;
        [FieldOffset(3)] public byte mouse3;
    }
}
