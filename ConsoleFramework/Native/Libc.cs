using System;
using System.Runtime.InteropServices;

namespace ConsoleFramework.Native;

/// <summary>
/// Interops for libc linux library.
/// Used to interoperate with eventfd polling (linux analogue of WaitForMultipleObjects).
/// </summary>
public static partial class Libc
{
    public const int LC_ALL = 0;

    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial string setlocale(int category, string locale);

    /// <summary>
    /// See the &lt;sys/poll.h&gt; and &lt;bits/poll.h&gt;
    /// </summary>
    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial int poll([In, Out] pollfd[] fds, int fdsCount, int timeout);

    /// <summary>
    /// Creates a pipe object. fds must be initialized array of 2 items.
    /// fds[0] will store descriptor for reading
    /// fds[1] will store descriptor for writing
    /// </summary>
    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial int pipe(int[] fds);

    /// <summary>
    /// Creates the eventfd kernel object. Returns file descriptor for
    /// created eventfd object.
    /// </summary>
    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial int eventfd(uint initval, EVENTFD_FLAGS flags);

    [LibraryImport("libc.so.6", SetLastError = true)]
    private static partial int read(int fd, out ulong buf, int count);

    [LibraryImport("libc.so.6", SetLastError = true)]
    private static partial int write(int fd, ref ulong buf, int count);

    /// <summary>
    /// Used to read from eventfd file descriptor.
    /// </summary>
    /// <returns>
    /// Number of bytes readed or -1 if error has occured.
    /// </returns>
    public static int readInt64(int fd, out ulong res)
    {
        return read(fd, out res, sizeof(ulong));
    }

    /// <summary>
    /// Used to write to eventfd file descriptor.
    /// </summary>
    /// <returns>
    /// Number of bytes written or -1 if error has occured.
    /// </returns>
    public static int writeInt64(int fd, ulong u)
    {
        return write(fd, ref u, sizeof(ulong));
    }

    /// <summary>
    /// Close the specified file descriptor.
    /// </summary>
    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial int close(int fd);

    // Used in terminal size retrieving
    public const int STDIN_FILENO = 0;
    public const int STDOUT_FILENO = 1;

    // For Linux it hardcoded to this constant
    public const int TIOCGWINSZ_LINUX = 0x5413;

    // For Mac OS it is different
    // https://groups.google.com/forum/#!msg/golang-nuts/eZgB_2RUDmQ/nv9wgeIoja4J
    public const int TIOCGWINSZ_DARWIN = 0x40087468;

    /// <summary>
    /// Used in terminal size retrieving.
    /// </summary>
    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial int ioctl(int fd, int cmd, out winsize ws);

    /// <summary>
    /// Interrupted system call. If after poll() error code is EINTR, this means
    /// that a signal was caught during poll().
    /// </summary>
    public const int EINTR = 4;

    /// <summary>
    /// Not a typewriter (fd is not associated with a terminal).
    /// </summary>
    public const int ENOTTY = 25;

    /// <summary>
    /// Returns actual terminal width and height.
    /// </summary>
    /// <param name="isDarwin">True if application is executed under Mac OS X.</param>
    /// <returns></returns>
    public static winsize GetTerminalSize(bool isDarwin)
    {
        ioctl(STDIN_FILENO, isDarwin ? TIOCGWINSZ_DARWIN : TIOCGWINSZ_LINUX, out winsize ws);
        return ws;
    }

    public delegate void SignalHandler(int arg);

    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial IntPtr signal(int signum, SignalHandler handler);

    /// <summary>
    /// Returns 1 if the file descriptor refers to a terminal.
    /// Returns 0 otherwise.
    /// </summary>
    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial int isatty(int fd);

    /// <summary>
    /// Retrieves terminal parameters into termios structure.
    /// </summary>
    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial int tcgetattr(int fd, [Out] out termios termios);

    /// <summary>
    /// Upon successful completion, the functions tcgetattr() and tcsetattr() 
    /// return a value of 0.  Otherwise, they return -1 and the global variable
    /// errno is set to indicate the error.
    /// </summary>
    [LibraryImport("libc.so.6", SetLastError = true)]
    public static partial int tcsetattr(int fd, int optional_actions, ref termios termios);

    /// <summary>
    /// The change occurs immediately.
    /// </summary>
    public const int TCSANOW = 0;

    /// <summary>
    /// The change occurs after all output written to fd has been transmitted.
    /// This function should be used when changing parameters that affect output.
    /// </summary>
    public const int TCSADRAIN = 1;

    /// <summary>
    /// The change occurs after all output written to the object referred by fd has been transmitted,
    /// and all input that has been received but not read will be discarded before the change is made.
    /// </summary>
    public const int TCSAFLUSH = 2;
}
