using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{


    // this layout is actual for 64-bit version of libtermkey.so

    [StructLayout(LayoutKind.Explicit)]
    public struct TermKeyKey64
    {
        [FieldOffset(0)] public TermKeyType type;
        // sizeof(code) must be 4, but if use a Sequential layout
        // it will be 8, so we have to explicitly specify the offsets
        [FieldOffset(8)] public code code;
        [FieldOffset(16)] public int modifiers;

        /* char[7] = Any Unicode character can be UTF-8 encoded in no more than 6 bytes, plus terminating NUL */
        [FieldOffset(20 + 0)] public byte utf8_0;
        [FieldOffset(20 + 1)] public byte utf8_1;
        [FieldOffset(20 + 2)] public byte utf8_2;
        [FieldOffset(20 + 3)] public byte utf8_3;
        [FieldOffset(20 + 4)] public byte utf8_4;
        [FieldOffset(20 + 5)] public byte utf8_5;
        [FieldOffset(20 + 6)] public byte utf8_6;
    }
}
