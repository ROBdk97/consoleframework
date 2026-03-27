using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ConsoleFramework.Native;

/// <summary>
/// Interop code for NCurses linux library. NCurses is used for graphics output only.
/// For keyboard and mouse input LibTermKey library is used.
/// </summary>
public static partial class NCurses
{
    private const string NativeLibraryName = "ncursesw";

    static NCurses()
    {
        NativeInteropResolver.EnsureInitialized(typeof(NCurses).Assembly);
    }

    /// <summary>
    /// Returns pointer to the WINDOW struct.
    /// If no terms has created manually returns stdscr.
    /// stdscr is used to set colors and attributes.
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial IntPtr initscr();

    /// <summary>
    /// Enables the keypad of the user's terminal.
    /// If enabled, the mouse events will be interpreted as mouse events
    /// (prefixed with KEY_MOUSE). Otherwise, mouse events cannot be
    /// correctly interpreted (garbage key codes in getch).
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial int keypad(IntPtr window, [MarshalAs(UnmanagedType.Bool)] bool bf);

    /// <summary>
    /// Enters the cbreak mode (no lines buffering in input).
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial int cbreak();

    /// <summary>
    /// Should be called to disable echo.
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial int noecho();

    /// <summary>
    /// To avoid that addch('\\') affects current symbol position.
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial int nonl();

    /// <summary>
    /// Set this option to true to avoid problems with keyboard input buffer
    /// flushing and inconsistent data displaying.
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial int intrflush(IntPtr window, [MarshalAs(UnmanagedType.Bool)] bool bf);

    [LibraryImport(NativeLibraryName)]
    internal static partial void refresh();

    [LibraryImport(NativeLibraryName)]
    internal static partial void clear();

    [LibraryImport(NativeLibraryName)]
    internal static partial int getch();

    /// <summary>
    /// Set specified cursor visibility.
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial int curs_set(CursorVisibility cursorVisibility);

    /// <summary>
    /// Moves the cursor associated with the window to line y and column x.
    /// This routine does not move the physical cursor of the terminal until refresh is called.
    /// The position specified is relative to the upper left-hand corner of the window, which is (0,0).
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial int move(int y, int x);

    [LibraryImport(NativeLibraryName)]
    internal static partial void endwin();

    /// <summary>
    /// We should call this function right after initscr()
    /// to enable the color subsystem.
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    internal static partial int start_color();

    [LibraryImport(NativeLibraryName)]
    internal static partial int init_color(short color, short r, short g, short b);

    [LibraryImport(NativeLibraryName)]
    internal static partial int init_pair(short i, short foregroundColor, short backgroundColor);

    [LibraryImport(NativeLibraryName)]
    internal static partial int addstr(string str);

    // Unfortunately there is no another way to marshal strings as UTF-8 in .NET Core
    [LibraryImport(NativeLibraryName)]
    internal static partial int mvaddstr(int x, int y, byte[] str);

    // doesn't work with UTF
    [LibraryImport(NativeLibraryName)]
    internal static partial int mvaddch(int x, int y, char ch);

    [LibraryImport(NativeLibraryName)]
    internal static partial int attron(int attrs);

    [LibraryImport(NativeLibraryName)]
    internal static partial int attrset(int attrs);

    [LibraryImport(NativeLibraryName)]
    internal static partial int color_set(short color, IntPtr opts);

    [LibraryImport(NativeLibraryName)]
    internal static partial int attroff(int attrs);

    internal const short NCURSES_ATTR_SHIFT = 8;

    internal static ulong NCURSES_BITS(ulong mask, short shift)
    {
        return (mask << (shift + NCURSES_ATTR_SHIFT));
    }

    internal static ulong COLOR_PAIR(short n)
    {
        return NCURSES_BITS((ulong)n, 0);
    }

    #region Predefined colors

    internal const short COLOR_BLACK = 0;
    internal const short COLOR_RED = 1;
    internal const short COLOR_GREEN = 2;
    internal const short COLOR_YELLOW = 3;
    internal const short COLOR_BLUE = 4;
    internal const short COLOR_MAGENTA = 5;
    internal const short COLOR_CYAN = 6;
    internal const short COLOR_WHITE = 7;

    internal static ulong A_STANDOUT = NCURSES_BITS(1UL, 8);
    internal static ulong A_UNDERLINE = NCURSES_BITS(1UL, 9);
    internal static ulong A_REVERSE = NCURSES_BITS(1UL, 10);
    internal static ulong A_BLINK = NCURSES_BITS(1UL, 11);
    internal static ulong A_DIM = NCURSES_BITS(1UL, 12);
    internal static ulong A_BOLD = NCURSES_BITS(1UL, 13);

    //

    #endregion

    /// <summary>
    /// Returns ncurses standard color for specified rgb combination.
    /// </summary>
    internal static int getStandardColor(bool r, bool g, bool b)
    {
        if (r)
        {
            if (g)
            {
                if (b)
                    return COLOR_WHITE;
                else
                    return COLOR_YELLOW; // must be brown ?
            }
            else
            {
                if (b)
                    return COLOR_MAGENTA;
                else
                    return COLOR_RED;
            }
        }
        else
        {
            if (g)
            {
                if (b)
                    return COLOR_CYAN;
                else
                    return COLOR_GREEN;
            }
            else
            {
                if (b)
                    return COLOR_BLUE;
                else
                    return COLOR_BLACK;
            }
        }
    }

    /// <summary>
    /// Doesn't support background intensity and other
    /// extended windows attributes.
    /// </summary>
    internal static short winAttrsToNCursesAttrs(Attr attrs, out bool fgIntensity)
    {
        bool fgRed = (attrs & Attr.FOREGROUND_RED) == Attr.FOREGROUND_RED;
        bool fgGreen = (attrs & Attr.FOREGROUND_GREEN) == Attr.FOREGROUND_GREEN;
        bool fgBlue = (attrs & Attr.FOREGROUND_BLUE) == Attr.FOREGROUND_BLUE;

        bool bgRed = (attrs & Attr.BACKGROUND_RED) == Attr.BACKGROUND_RED;
        bool bgGreen = (attrs & Attr.BACKGROUND_GREEN) == Attr.BACKGROUND_GREEN;
        bool bgBlue = (attrs & Attr.BACKGROUND_BLUE) == Attr.BACKGROUND_BLUE;

        fgIntensity = (attrs & Attr.FOREGROUND_INTENSITY) == Attr.FOREGROUND_INTENSITY;

        int fg = getStandardColor(fgRed, fgGreen, fgBlue);
        int bg = getStandardColor(bgRed, bgGreen, bgBlue);

        //short index = (short) (fg | (bg << 3));
        //if (!usedIndexes.Contains(index)) {
        //    init_pair(index, (short) fg, (short) bg);
        //    usedIndexes.Add(index);
        //}
        int index = (fg | (bg << 3));
        if (createdPairs.TryGetValue(index, out short value))
        {
            return value;
        }
        else
        {
            short pairId = (short)(lastUsedPairId + 1);
            init_pair(pairId, (short)fg, (short)bg);
            createdPairs.Add(index, pairId);
            lastUsedPairId++;
            return pairId;
        }
    }

    private static short lastUsedPairId = 0;
    private static readonly Dictionary<int, short> createdPairs = [];

    #region Mouse-related stuff

    /// <summary>
    /// Since there is no information about argument types in ncurses headers,
    /// let's try to use type inference for generic methods.
    /// </summary>
    private static ulong NCURSES_MOUSE_MASK(int b, ulong m)
    {
        return ((m) << (((b) - 1) * 6));
    }

    internal const ulong NCURSES_BUTTON_RELEASED = 001L;
    internal const ulong NCURSES_BUTTON_PRESSED = 002L;
    internal const ulong NCURSES_BUTTON_CLICKED = 004L;
    internal const ulong NCURSES_DOUBLE_CLICKED = 010L;
    internal const ulong NCURSES_TRIPLE_CLICKED = 020L;
    internal const ulong NCURSES_RESERVED_EVENT = 040L;

    internal static ulong BUTTON1_RELEASED = NCURSES_MOUSE_MASK(1, NCURSES_BUTTON_RELEASED);

    internal static ulong BUTTON1_PRESSED = NCURSES_MOUSE_MASK(1, NCURSES_BUTTON_PRESSED);
    internal static ulong BUTTON1_CLICKED = NCURSES_MOUSE_MASK(1, NCURSES_BUTTON_CLICKED);
    internal static ulong BUTTON1_DOUBLE_CLICKED = NCURSES_MOUSE_MASK(1, NCURSES_DOUBLE_CLICKED);
    internal static ulong BUTTON1_TRIPLE_CLICKED = NCURSES_MOUSE_MASK(1, NCURSES_TRIPLE_CLICKED);

    internal static ulong BUTTON2_RELEASED = NCURSES_MOUSE_MASK(2, NCURSES_BUTTON_RELEASED);
    internal static ulong BUTTON2_PRESSED = NCURSES_MOUSE_MASK(2, NCURSES_BUTTON_PRESSED);
    internal static ulong BUTTON2_CLICKED = NCURSES_MOUSE_MASK(2, NCURSES_BUTTON_CLICKED);
    internal static ulong BUTTON2_DOUBLE_CLICKED = NCURSES_MOUSE_MASK(2, NCURSES_DOUBLE_CLICKED);
    internal static ulong BUTTON2_TRIPLE_CLICKED = NCURSES_MOUSE_MASK(2, NCURSES_TRIPLE_CLICKED);

    internal static ulong BUTTON3_RELEASED = NCURSES_MOUSE_MASK(3, NCURSES_BUTTON_RELEASED);
    internal static ulong BUTTON3_PRESSED = NCURSES_MOUSE_MASK(3, NCURSES_BUTTON_PRESSED);
    internal static ulong BUTTON3_CLICKED = NCURSES_MOUSE_MASK(3, NCURSES_BUTTON_CLICKED);
    internal static ulong BUTTON3_DOUBLE_CLICKED = NCURSES_MOUSE_MASK(3, NCURSES_DOUBLE_CLICKED);
    internal static ulong BUTTON3_TRIPLE_CLICKED = NCURSES_MOUSE_MASK(3, NCURSES_TRIPLE_CLICKED);

    internal static ulong BUTTON4_RELEASED = NCURSES_MOUSE_MASK(4, NCURSES_BUTTON_RELEASED);
    internal static ulong BUTTON4_PRESSED = NCURSES_MOUSE_MASK(4, NCURSES_BUTTON_PRESSED);
    internal static ulong BUTTON4_CLICKED = NCURSES_MOUSE_MASK(4, NCURSES_BUTTON_CLICKED);
    internal static ulong BUTTON4_DOUBLE_CLICKED = NCURSES_MOUSE_MASK(4, NCURSES_DOUBLE_CLICKED);
    internal static ulong BUTTON4_TRIPLE_CLICKED = NCURSES_MOUSE_MASK(4, NCURSES_TRIPLE_CLICKED);

    internal static ulong BUTTON_CTRL = NCURSES_MOUSE_MASK(5, 0001L);
    internal static ulong BUTTON_SHIFT = NCURSES_MOUSE_MASK(5, 0002L);
    internal static ulong BUTTON_ALT = NCURSES_MOUSE_MASK(5, 0004L);
    internal static ulong REPORT_MOUSE_POSITION = NCURSES_MOUSE_MASK(5, 0010L);

    internal static ulong ALL_MOUSE_EVENTS = (REPORT_MOUSE_POSITION - 1);

    /* macros to extract single event-bits from masks */
    internal static bool BUTTON_RELEASE(ulong e, int x) { return ((e) & NCURSES_MOUSE_MASK(x, 001)) != 0; }
    internal static bool BUTTON_PRESS(ulong e, int x) { return ((e) & NCURSES_MOUSE_MASK(x, 002)) != 0; }
    internal static bool BUTTON_CLICK(ulong e, int x) { return ((e) & NCURSES_MOUSE_MASK(x, 004)) != 0; }
    internal static bool BUTTON_DOUBLE_CLICK(ulong e, int x) { return ((e) & NCURSES_MOUSE_MASK(x, 010)) != 0; }
    internal static bool BUTTON_TRIPLE_CLICK(ulong e, int x) { return ((e) & NCURSES_MOUSE_MASK(x, 020)) != 0; }
    internal static bool BUTTON_RESERVED_EVENT(ulong e, int x) { return ((e) & NCURSES_MOUSE_MASK(x, 040)) != 0; }

    [LibraryImport(NativeLibraryName)]
    internal static partial ulong mousemask(ulong mask, IntPtr currentMaskPtr);

    internal const int KEY_MOUSE = 409;

    #endregion
}
