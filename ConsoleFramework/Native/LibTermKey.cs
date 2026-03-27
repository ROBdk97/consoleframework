using System;
using System.Runtime.InteropServices;

namespace ConsoleFramework.Native;

/// <summary>
/// Interop code for LibTermKey linux library.
/// LibTermKey is used for handling keyboard and mouse input.
/// </summary>
public static partial class LibTermKey
{
    private const string NativeLibraryName = "termkey";

    static LibTermKey()
    {
        NativeInteropResolver.EnsureInitialized(typeof(LibTermKey).Assembly);
    }

    [LibraryImport(NativeLibraryName)]
    internal static partial IntPtr termkey_new(int fd, TermKeyFlag flags);

    internal static TermKeyResult termkey_getkey(IntPtr termKey, ref TermKeyKey key)
    {
        if (IntPtr.Size == 4)
        {
            TermKeyKey32 temp = new();
            TermKeyResult result = termkey_getkey32(termKey, ref temp);
            key.type = temp.type;
            key.code = temp.code;
            key.modifiers = temp.modifiers;
            key.utf8_0 = temp.utf8_0;
            key.utf8_1 = temp.utf8_1;
            key.utf8_2 = temp.utf8_2;
            key.utf8_3 = temp.utf8_3;
            key.utf8_4 = temp.utf8_4;
            key.utf8_5 = temp.utf8_5;
            key.utf8_6 = temp.utf8_6;
            return result;
        }
        else
        {
            TermKeyKey64 temp = new();
            TermKeyResult result = termkey_getkey64(termKey, ref temp);
            key.type = temp.type;
            key.code = temp.code;
            key.modifiers = temp.modifiers;
            key.utf8_0 = temp.utf8_0;
            key.utf8_1 = temp.utf8_1;
            key.utf8_2 = temp.utf8_2;
            key.utf8_3 = temp.utf8_3;
            key.utf8_4 = temp.utf8_4;
            key.utf8_5 = temp.utf8_5;
            key.utf8_6 = temp.utf8_6;
            return result;
        }
    }

    [LibraryImport(NativeLibraryName, EntryPoint = "termkey_getkey")]
    private static partial TermKeyResult termkey_getkey32(IntPtr termKey, ref TermKeyKey32 key);

    [LibraryImport(NativeLibraryName, EntryPoint = "termkey_getkey")]
    private static partial TermKeyResult termkey_getkey64(IntPtr termKey, ref TermKeyKey64 key);

    internal static TermKeyResult termkey_getkey_force(IntPtr termKey, ref TermKeyKey key)
    {
        if (IntPtr.Size == 4)
        {
            TermKeyKey32 temp = new();
            TermKeyResult result = termkey_getkey_force32(termKey, ref temp);
            key.type = temp.type;
            key.code = temp.code;
            key.modifiers = temp.modifiers;
            key.utf8_0 = temp.utf8_0;
            key.utf8_1 = temp.utf8_1;
            key.utf8_2 = temp.utf8_2;
            key.utf8_3 = temp.utf8_3;
            key.utf8_4 = temp.utf8_4;
            key.utf8_5 = temp.utf8_5;
            key.utf8_6 = temp.utf8_6;
            return result;
        }
        else
        {
            TermKeyKey64 temp = new();
            TermKeyResult result = termkey_getkey_force64(termKey, ref temp);
            key.type = temp.type;
            key.code = temp.code;
            key.modifiers = temp.modifiers;
            key.utf8_0 = temp.utf8_0;
            key.utf8_1 = temp.utf8_1;
            key.utf8_2 = temp.utf8_2;
            key.utf8_3 = temp.utf8_3;
            key.utf8_4 = temp.utf8_4;
            key.utf8_5 = temp.utf8_5;
            key.utf8_6 = temp.utf8_6;
            return result;
        }
    }

    [LibraryImport(NativeLibraryName, EntryPoint = "termkey_getkey_force")]
    private static partial TermKeyResult termkey_getkey_force32(IntPtr termKey, ref TermKeyKey32 key);

    [LibraryImport(NativeLibraryName, EntryPoint = "termkey_getkey_force")]
    private static partial TermKeyResult termkey_getkey_force64(IntPtr termKey, ref TermKeyKey64 key);

    [LibraryImport(NativeLibraryName)]
    internal static partial TermKeyResult termkey_advisereadable(IntPtr termKey);

    [LibraryImport(NativeLibraryName)]
    internal static partial int termkey_get_waittime(IntPtr termkey);

    [LibraryImport(NativeLibraryName)]
    internal static partial void termkey_destroy(IntPtr termkey);

    internal static TermKeyResult termkey_interpret_mouse(IntPtr termKey, ref TermKeyKey key,
                                                                out TermKeyMouseEvent ev,
                                                                out int button,
                                                                out int line,
                                                                out int col)
    {
        if (IntPtr.Size == 4)
        {
            TermKeyKey32 temp = new()
            {
                type = key.type,
                code = key.code,
                modifiers = key.modifiers,
                utf8_0 = key.utf8_0,
                utf8_1 = key.utf8_1,
                utf8_2 = key.utf8_2,
                utf8_3 = key.utf8_3,
                utf8_4 = key.utf8_4,
                utf8_5 = key.utf8_5,
                utf8_6 = key.utf8_6
            };

            TermKeyResult result = termkey_interpret_mouse32(termKey, ref temp, out ev, out button, out line, out col);

            key.type = temp.type;
            key.code = temp.code;
            key.modifiers = temp.modifiers;
            key.utf8_0 = temp.utf8_0;
            key.utf8_1 = temp.utf8_1;
            key.utf8_2 = temp.utf8_2;
            key.utf8_3 = temp.utf8_3;
            key.utf8_4 = temp.utf8_4;
            key.utf8_5 = temp.utf8_5;
            key.utf8_6 = temp.utf8_6;

            return result;
        }
        else
        {
            TermKeyKey64 temp = new()
            {
                type = key.type,
                code = key.code,
                modifiers = key.modifiers,
                utf8_0 = key.utf8_0,
                utf8_1 = key.utf8_1,
                utf8_2 = key.utf8_2,
                utf8_3 = key.utf8_3,
                utf8_4 = key.utf8_4,
                utf8_5 = key.utf8_5,
                utf8_6 = key.utf8_6
            };

            TermKeyResult result = termkey_interpret_mouse64(termKey, ref temp, out ev, out button, out line, out col);

            key.type = temp.type;
            key.code = temp.code;
            key.modifiers = temp.modifiers;
            key.utf8_0 = temp.utf8_0;
            key.utf8_1 = temp.utf8_1;
            key.utf8_2 = temp.utf8_2;
            key.utf8_3 = temp.utf8_3;
            key.utf8_4 = temp.utf8_4;
            key.utf8_5 = temp.utf8_5;
            key.utf8_6 = temp.utf8_6;

            return result;
        }
    }

    [LibraryImport(NativeLibraryName, EntryPoint = "termkey_interpret_mouse")]
    private static partial TermKeyResult termkey_interpret_mouse32(IntPtr termKey, ref TermKeyKey32 key,
                                                                out TermKeyMouseEvent ev,
                                                                out int button,
                                                                out int line,
                                                                out int col);

    [LibraryImport(NativeLibraryName, EntryPoint = "termkey_interpret_mouse")]
    private static partial TermKeyResult termkey_interpret_mouse64(IntPtr termKey, ref TermKeyKey64 key,
                                                                out TermKeyMouseEvent ev,
                                                                out int button,
                                                                out int line,
                                                                out int col);
}
