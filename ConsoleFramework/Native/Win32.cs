using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleFramework.Native;

/// <summary>
/// Interop code for Win32 environment.
/// </summary>
public static partial class Win32
{
    public const uint INFINITE = 0xFFFFFFFF;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    internal static partial void AllocConsole();

    /// <summary>
    /// Returns current console mode. Program saves it before changing and
    /// restores before exit.
    /// </summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);

    /// <summary>
    /// It is used to set ENABLE_WINDOW_INPUT flag, which enables the events
    /// about console screen buffer resize.
    /// </summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
        out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    internal static partial IntPtr GetStdHandle([MarshalAs(UnmanagedType.I4)] StdHandleType nStdHandle);

    [LibraryImport("kernel32.dll")]
    internal static partial uint WaitForMultipleObjects(uint nCount,
                                                     [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] lpHandles,
                                                     [MarshalAs(UnmanagedType.Bool)] bool bWaitAll, uint dwMilliseconds);

    [LibraryImport("kernel32.dll", EntryPoint = "ReadConsoleInputW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ReadConsoleInput(IntPtr hConsoleInput,
                                                [Out] INPUT_RECORD[] lpBuffer,
                                                uint nLength, out uint lpNumberOfEventsRead);

    [LibraryImport("kernel32.dll", EntryPoint = "WriteConsoleOutputW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool WriteConsoleOutputCore(IntPtr hConsoleOutput, CHAR_INFO[,] lpBuffer, COORD dwBufferSize,
                                                  COORD dwBufferCoord, ref SMALL_RECT lpWriteRegion);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetConsoleCursorInfo(IntPtr hConsoleOutput, out CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetConsoleCursorInfo(IntPtr hConsoleOutput, ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int FormatMessage(int dwFlags, string lpSource, int dwMessageId, int dwLanguageId,
                                           StringBuilder lpBuffer, int nSize, string[] Arguments);

    public static string GetLastErrorMessage()
    {
        StringBuilder strLastErrorMessage = new(255);
        int ret2 = Marshal.GetLastWin32Error();
        const int dwFlags = 4096;
        _ = FormatMessage(dwFlags, null, ret2, 0, strLastErrorMessage, strLastErrorMessage.Capacity, null);
        return strLastErrorMessage.ToString();
    }

    [LibraryImport("user32.dll")]
    internal static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("kernel32.dll")]
    internal static partial IntPtr GetConsoleWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool IsZoomed(IntPtr hwnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool IsIconic(IntPtr hwnd);

    public const uint WM_SYSCOMMAND = 0x0112;

    public static readonly IntPtr SC_MAXIMIZE = new(0xF030);

    public static readonly IntPtr SC_RESTORE = new(0xF120);
}

