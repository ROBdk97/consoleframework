using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using ConsoleFramework.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConsoleFramework;

/// <summary>
/// Console application entry point.
/// Encapsulates messages loop and application lifecycle.
/// Supports Windows and Linux.
/// </summary>
public sealed class ConsoleApplication : IDisposable
{
    private bool maximized;
    private Size savedBufferSize;
    private Rect savedWindowRect;

    private IntPtr consoleWindowHwnd;
    private IntPtr getConsoleWindowHwnd()
    {
        if (IntPtr.Zero == consoleWindowHwnd)
        {
            consoleWindowHwnd = Win32.GetConsoleWindow();
        }
        return consoleWindowHwnd;
    }

    /// <summary>
    /// Maximizes the terminal window size and terminal buffer size.
    /// Current size is stored.
    /// </summary>
    public void Maximize()
    {
        if (usingLinux)
        {
            // Doesn't work in Konsole
            Console.Write("\x1B[9;1t");
            return;
        }

        if (maximized) return;
        //
        savedBufferSize = new Size(Console.BufferWidth, Console.BufferHeight);
        Win32.SendMessage(getConsoleWindowHwnd(), Win32.WM_SYSCOMMAND,
            Win32.SC_MAXIMIZE, IntPtr.Zero);
#pragma warning disable CA1416
        int maxWidth = Console.LargestWindowWidth;
        int maxHeight = Console.LargestWindowHeight;
        Console.SetWindowPosition(0, 0);
        Console.SetBufferSize(maxWidth, maxHeight);
        Console.SetWindowSize(maxWidth, maxHeight);
#pragma warning restore CA1416

        // Apply new sizes to Canvas
        CanvasSize = new Size(maxWidth, maxHeight);
        renderer.RootElementRect = new Rect(canvas.Size);
        renderer.UpdateLayout();

        maximized = true;
    }

    /// <summary>
    /// Restores the terminal window size and terminal buffer to stored state.
    /// </summary>
    public void Restore()
    {
        if (usingLinux)
        {
            // Doesn't work in Konsole
            Console.Write("\x1B[9;0t");
            return;
        }

        if (!maximized) return;
        //
        Win32.SendMessage(getConsoleWindowHwnd(), Win32.WM_SYSCOMMAND,
            Win32.SC_RESTORE, IntPtr.Zero);
#pragma warning disable CA1416
        Console.SetWindowPosition(0, 0);

        // Get largest size again - because resolution of screen can change
        // between maximize and restore calls
        int maxWidth = Console.LargestWindowWidth;
        int maxHeight = Console.LargestWindowHeight;

        Console.SetWindowSize(
            Math.Min(savedWindowRect.Width, maxWidth),
            Math.Min(savedWindowRect.Height, maxHeight));
        Console.SetWindowPosition(savedWindowRect.Left, savedWindowRect.Top);
#pragma warning restore CA1416

        // Apply new sizes to Canvas
        CanvasSize = new Size(savedWindowRect.Width, savedWindowRect.Height);
        renderer.RootElementRect = new Rect(canvas.Size);
        renderer.UpdateLayout();

        maximized = false;
    }

    /// <summary>
    /// Fires when console buffer size is changed.
    /// </summary>
    public event TerminalSizeChangedHandler TerminalSizeChanged;

    /// <summary>
    /// Default TerminalSizeChanged event handler. Invoked when
    /// initial CanvasSize and RootElementRect are empty and no another
    /// TerminalSizeChanged handler is attached.
    /// </summary>
    public void OnTerminalSizeChangedDefault(object sender, TerminalSizeChangedEventArgs args)
    {
        if (!userCanvasSize.IsEmpty) throw new InvalidOperationException("Assertion failed.");
        if (!userRootElementRect.IsEmpty) throw new InvalidOperationException("Assertion failed.");
        if (TerminalSizeChanged != null) throw new InvalidOperationException("Assertion failed.");

        canvas.Size = new Size(args.Width, args.Height);
        renderer.RootElementRect = new Rect(canvas.Size);
        renderer.UpdateLayout();
    }

    private Size userCanvasSize;

    /// <summary>
    /// Gets or sets a size of canvas. Whet set, old canvas image will be
    /// copied to new one.
    /// </summary>
    public Size CanvasSize
    {
        get
        {
            if (running && userCanvasSize.IsEmpty)
                return canvas.Size;
            return userCanvasSize;
        }
        set
        {
            if (running && value != canvas.Size)
            {
                canvas.Size = value;
            }
            userCanvasSize = value;
        }
    }

    private Rect userRootElementRect;
    /// <summary>
    /// Gets or sets the root element rect.
    /// When set, root element will be added to invalidation queue automatically.
    /// </summary>
    public Rect RootElementRect
    {
        get
        {
            if (running && userRootElementRect.IsEmpty)
            {
                return renderer.RootElementRect;
            }
            return userRootElementRect;
        }
        set
        {
            if (running && value != renderer.RootElementRect)
            {
                renderer.RootElementRect = value;
            }
            userRootElementRect = value;
        }
    }

    private volatile bool running;
    private PhysicalCanvas canvas;

    public static Control LoadFromXaml(string xamlResourceName, object dataContext)
    {
        var assembly = Assembly.GetEntryAssembly();
        using Stream stream = assembly.GetManifestResourceStream(xamlResourceName);
        if (null == stream)
        {
            throw new ArgumentException("Resource not found.", nameof(xamlResourceName));
        }
        using StreamReader reader = new(stream);
        string result = reader.ReadToEnd();
        Control control = XamlParser.CreateFromXaml<Control>(result, dataContext,
            [
                "clr-namespace:Xaml;assembly=ConsoleFramework",
                        "clr-namespace:ConsoleFramework.Xaml;assembly=ConsoleFramework",
                        "clr-namespace:ConsoleFramework.Controls;assembly=ConsoleFramework",
                    ]);
        control.DataContext = dataContext;
        control.Created();
        return control;
    }

    private static readonly bool usingLinux;
    private static readonly bool isDarwin;

    static ConsoleApplication()
    {
        usingLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        isDarwin = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    private ConsoleApplication()
    {
        eventManager = new EventManager();
        focusManager = new FocusManager(eventManager);

        exitWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        invokeWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
    }

    private static volatile ConsoleApplication instance;
    private static readonly Lock syncRoot = new();

    /// <summary>
    /// Instance of Application object.
    /// </summary>
    public static ConsoleApplication Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    instance ??= new ConsoleApplication();
                }
            }
            return instance;
        }
    }

    private IntPtr stdInputHandle;
    private IntPtr stdOutputHandle;
    private readonly EventWaitHandle exitWaitHandle;
    private readonly EventWaitHandle invokeWaitHandle;
    private int? mainThreadId;

    private struct ActionInfo
    {
        public readonly Action action;
        public readonly EventWaitHandle waitHandle;

        public ActionInfo(Action action, EventWaitHandle waitHandle)
        {
            this.action = action;
            this.waitHandle = waitHandle;
        }
    }

    private readonly List<ActionInfo> actionsToBeInvoked = [];
    private readonly Lock actionsLocker = new();

    /// <summary>
    /// Signals the message loop to be finished.
    /// Application shutdowns after that.
    /// </summary>
    public void Exit()
    {
        if (usingLinux)
        {
            int res = Libc.writeInt64(pipeFds[1], 1);
            if (-1 == res) throw new InvalidOperationException("Cannot write to self-pipe.");
        }
        else
        {
            exitWaitHandle.Set();
        }
    }

    private readonly Renderer renderer = new();
    public Renderer Renderer
    {
        get
        {
            return renderer;
        }
    }

    /// <summary>
    /// Returns the root control of the application.
    /// </summary>
    public Control RootControl
    {
        get { return mainControl; }
    }

    private Control mainControl;
    private readonly EventManager eventManager;
    private readonly FocusManager focusManager;

    public FocusManager FocusManager
    {
        get
        {
            return focusManager;
        }
    }

    public EventManager EventManager
    {
        get
        {
            return eventManager;
        }
    }

    internal void SetCursorPosition(Point position)
    {
        if (!usingLinux)
        {
            Win32.SetConsoleCursorPosition(stdOutputHandle, new COORD((short)position.x, (short)position.y));
        }
        else
        {
            _ = NCurses.move(position.y, position.x);
            NCurses.refresh();
        }
    }

    /// <summary>
    /// The state of the console cursor to avoid repeated Show and Hide calls.
    /// The consistency of this property can be violated if native functions are used
    /// for working with the cursor directly in the application.
    /// </summary>
    internal bool CursorIsVisible
    {
        get;
        private set;
    }

    /// <summary>
    /// Makes the console cursor visible and sets the CursorIsVisible value to true.
    /// </summary>
    internal void ShowCursor()
    {
        if (!usingLinux)
        {
            CONSOLE_CURSOR_INFO consoleCursorInfo = new()
            {
                Size = 5,
                Visible = true
            };
            Win32.SetConsoleCursorInfo(stdOutputHandle, ref consoleCursorInfo);
        }
        else
        {
            _ = NCurses.curs_set(CursorVisibility.Visible);
        }
        CursorIsVisible = true;
    }

    /// <summary>
    /// Makes the console cursor invisible and sets the
    /// CursorIsVisible value to false.
    /// </summary>
    internal void HideCursor()
    {
        if (!usingLinux)
        {
            CONSOLE_CURSOR_INFO consoleCursorInfo = new()
            {
                Size = 5,
                Visible = false
            };
            Win32.SetConsoleCursorInfo(stdOutputHandle, ref consoleCursorInfo);
        }
        else
        {
            _ = NCurses.curs_set(CursorVisibility.Invisible);
        }
        CursorIsVisible = false;
    }

    /// <summary>
    /// Runs application using specified control as root control.
    /// Application will run until method <see cref="Exit"/> is called.
    /// </summary>
    /// <param name="control"></param>
    public void Run(Control control)
    {
        try
        {
            if (usingLinux)
            {
                runLinux(control);
            }
            else
            {
                runWindows(control);
            }
        }
        finally
        {
            running = false;
            mainThreadId = null;
        }
    }

    public void Run(Control control, Size canvasSize, Rect rectToUse)
    {
        userCanvasSize = canvasSize;
        userRootElementRect = rectToUse;
        Run(control);
    }

    /// <summary>
    /// File descriptors for self-pipe.
    /// First descriptor is used to read from pipe, second - to write.
    /// </summary>
    private readonly int[] pipeFds = new int[2];
    private IntPtr termkeyHandle = IntPtr.Zero;

    private void emitLinuxTerminalSizeChangedEvent()
    {
        INPUT_RECORD inputRecord = new()
        {
            EventType = EventType.WINDOW_BUFFER_SIZE_EVENT
        };

        winsize ws = Libc.GetTerminalSize(isDarwin);

        inputRecord.WindowBufferSizeEvent.dwSize.X = (short)ws.ws_col;
        inputRecord.WindowBufferSizeEvent.dwSize.Y = (short)ws.ws_row;
        processInputEvent(inputRecord);
    }

    private void emitWindowsTerminalSizeChangedEvent(int width, int height)
    {
        INPUT_RECORD inputRecord = new()
        {
            EventType = EventType.WINDOW_BUFFER_SIZE_EVENT
        };

        inputRecord.WindowBufferSizeEvent.dwSize.X = (short)width;
        inputRecord.WindowBufferSizeEvent.dwSize.Y = (short)height;
        processInputEvent(inputRecord);
    }

    private void runLinux(Control control)
    {
        mainControl = control;

        if (Console.IsInputRedirected || Console.IsOutputRedirected ||
            Libc.isatty(Libc.STDIN_FILENO) != 1 || Libc.isatty(Libc.STDOUT_FILENO) != 1)
        {
            throw new InvalidOperationException("Linux console mode requires an interactive terminal (TTY). Input/output are redirected. Run the app in a real terminal session instead of a redirected debugger console.");
        }

        if (userCanvasSize.IsEmpty)
        {
            // Create physical canvas with actual terminal size
            winsize ws = Libc.GetTerminalSize(isDarwin);
            canvas = new PhysicalCanvas(ws.ws_col, ws.ws_row);
        }
        else
        {
            canvas = new PhysicalCanvas(userCanvasSize.Width, userCanvasSize.Height);
        }
        renderer.Canvas = canvas;
        renderer.RootElementRect = userRootElementRect.IsEmpty
            ? new Rect(canvas.Size) : userRootElementRect;
        renderer.RootElement = mainControl;
        //
        mainControl.Invalidate();

        // Terminal initialization sequence

        // Because .NET Core runtime changes locale to something wrong on startup,
        // we have to change it to default system locale
        // See https://stackoverflow.com/a/6249265
        // And https://github.com/dotnet/coreclr/issues/1012
        Libc.setlocale(Libc.LC_ALL, string.Empty);

        // Save all terminal properties
        if (0 != Libc.tcgetattr(Libc.STDIN_FILENO, out termios termios))
        {
            int errorCode = Marshal.GetLastWin32Error();
            if (errorCode == Libc.ENOTTY)
            {
                throw new InvalidOperationException("Failed to initialize terminal mode: stdin is not a TTY. Run the app in an interactive terminal session.");
            }

            throw new InvalidOperationException(string.Format("Failed to call tcgetattr(). LastError is {0}", errorCode));
        }

        bool ncursesInitialized = false;
        bool inputModeEnabled = false;
        bool cursorHidden = false;
        bool pipeCreated = false;

        pipeFds[0] = -1;
        pipeFds[1] = -1;

        IntPtr stdscr = NCurses.initscr();
        if (stdscr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to initialize ncurses (initscr returned null).");
        }

        ncursesInitialized = true;
        _ = NCurses.cbreak();
        _ = NCurses.noecho();
        _ = NCurses.nonl();
        _ = NCurses.intrflush(stdscr, false);
        _ = NCurses.keypad(stdscr, true);
        _ = NCurses.start_color();

        HideCursor();
        cursorHidden = true;
        try
        {
            renderer.UpdateLayout();
            renderer.FinallyApplyChangesToCanvas();

            termkeyHandle = LibTermKey.termkey_new(Libc.STDIN_FILENO, TermKeyFlag.TERMKEY_FLAG_SPACESYMBOL);
            if (termkeyHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to initialize libtermkey (termkey_new returned null).");
            }

            // Setup the input mode
            Console.Write("\x1B[?1002h");
            inputModeEnabled = true;
            pollfd fd = new()
            {
                fd = Libc.STDIN_FILENO,
                events = POLL_EVENTS.POLLIN
            };

            pollfd[] fds = [fd, new pollfd()];
            int pipeResult = Libc.pipe(pipeFds);
            if (pipeResult == -1)
            {
                throw new InvalidOperationException("Cannot create self-pipe.");
            }

            pipeCreated = true;
            fds[1].fd = pipeFds[0];
            fds[1].events = POLL_EVENTS.POLLIN;

            try
            {
                TermKeyKey key = new();
                winsize previousTerminalSize = Libc.GetTerminalSize(isDarwin);
                //
                running = true;
                mainThreadId = Environment.CurrentManagedThreadId;
                //
                int nextwait = -1;
                while (true)
                {
                    int pollTimeout = nextwait == -1 ? 100 : nextwait;
                    int pollRes = Libc.poll(fds, 2, pollTimeout);
                    if (pollRes == 0)
                    {
                        if (nextwait != -1 &&
                            TermKeyResult.TERMKEY_RES_KEY == LibTermKey.termkey_getkey_force(termkeyHandle, ref key))
                        {
                            processLinuxInput(key);
                        }
                    }
                    if (pollRes == -1)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode != Libc.EINTR)
                        {
                            throw new InvalidOperationException(string.Format("poll() returned with error code {0}", errorCode));
                        }
                    }

                    if (fds[1].revents != POLL_EVENTS.NONE)
                    {
                        Libc.readInt64(fds[1].fd, out ulong u);
                        if (u == 1)
                        {
                            // Exit from application
                            break;
                        }
                        if (u == 3)
                        {
                            // It is signal from async actions invocation stuff
                        }
                    }

                    winsize currentTerminalSize = Libc.GetTerminalSize(isDarwin);
                    if (currentTerminalSize.ws_col != previousTerminalSize.ws_col ||
                        currentTerminalSize.ws_row != previousTerminalSize.ws_row)
                    {
                        emitLinuxTerminalSizeChangedEvent();
                        previousTerminalSize = currentTerminalSize;
                    }

                    if ((fds[0].revents & POLL_EVENTS.POLLIN) == POLL_EVENTS.POLLIN ||
                         (fds[0].revents & POLL_EVENTS.POLLHUP) == POLL_EVENTS.POLLHUP ||
                         (fds[0].revents & POLL_EVENTS.POLLERR) == POLL_EVENTS.POLLERR)
                    {
                        LibTermKey.termkey_advisereadable(termkeyHandle);
                    }

                    TermKeyResult result = (LibTermKey.termkey_getkey(termkeyHandle, ref key));
                    while (result == TermKeyResult.TERMKEY_RES_KEY)
                    {
                        processLinuxInput(key);
                        result = (LibTermKey.termkey_getkey(termkeyHandle, ref key));
                    }

                    if (result == TermKeyResult.TERMKEY_RES_AGAIN)
                    {
                        nextwait = LibTermKey.termkey_get_waittime(termkeyHandle);
                    }
                    else
                    {
                        nextwait = -1;
                    }

                    while (true)
                    {
                        bool anyInvokeActions = isAnyInvokeActions();
                        bool anyRoutedEvent = !EventManager.IsQueueEmpty();
                        bool anyLayoutToRevalidate = renderer.AnyControlInvalidated;

                        if (!anyInvokeActions && !anyRoutedEvent && !anyLayoutToRevalidate)
                            break;

                        EventManager.ProcessEvents();
                        processInvokeActions();
                        renderer.UpdateLayout();
                    }

                    renderer.FinallyApplyChangesToCanvas();
                }

            }
            finally
            {
                if (termkeyHandle != IntPtr.Zero)
                {
                    LibTermKey.termkey_destroy(termkeyHandle);
                    termkeyHandle = IntPtr.Zero;
                }

                if (pipeCreated)
                {
                    if (pipeFds[0] >= 0)
                    {
                        Libc.close(pipeFds[0]);
                        pipeFds[0] = -1;
                    }

                    if (pipeFds[1] >= 0)
                    {
                        Libc.close(pipeFds[1]);
                        pipeFds[1] = -1;
                    }
                }

                if (inputModeEnabled)
                {
                    Console.Write("\x1B[?1002l");
                }
            }
        }
        finally
        {
            // Restore cursor visibility before exit
            if (cursorHidden)
            {
                ShowCursor();
            }

            if (ncursesInitialized)
            {
                NCurses.endwin();
            }

            // Restore all terminal parameters
            // Don't throw from finally block to avoid masking other exceptions
            _ = Libc.tcsetattr(Libc.STDIN_FILENO, Libc.TCSANOW, ref termios);
        }

        renderer.RootElement = null;
    }

    private void processLinuxInput(TermKeyKey key)
    {
        // If any special button has been pressed (Tab, Enter, etc)
        // we should convert its code to INPUT_RECORD.KeyEvent
        // Because INPUT_RECORD.KeyEvent depends on Windows' scan codes,
        // we convert codes retrieved from LibTermKey to Windows virtual scan codes
        // In the future, this logic may be changed (for example, both Windows and Linux
        // raw codes can be converted into ConsoleFramework's own abstract enum)
        if (key.type == TermKeyType.TERMKEY_TYPE_KEYSYM)
        {
            INPUT_RECORD inputRecord = new()
            {
                EventType = EventType.KEY_EVENT
            };
            inputRecord.KeyEvent.bKeyDown = true;
            inputRecord.KeyEvent.wRepeatCount = 1;
            switch (key.code.sym)
            {
                case TermKeySym.TERMKEY_SYM_TAB:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Tab;
                    break;
                case TermKeySym.TERMKEY_SYM_ENTER:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Return;
                    break;
                // in gnome-terminal it is backspace by default
                // (see default compatibility settings in Profile's settings)
                case TermKeySym.TERMKEY_SYM_DEL:
                case TermKeySym.TERMKEY_SYM_BACKSPACE:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Back;
                    break;
                case TermKeySym.TERMKEY_SYM_DELETE:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Delete;
                    break;
                case TermKeySym.TERMKEY_SYM_HOME:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Home;
                    break;
                case TermKeySym.TERMKEY_SYM_END:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.End;
                    break;
                case TermKeySym.TERMKEY_SYM_PAGEUP:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Prior;
                    break;
                case TermKeySym.TERMKEY_SYM_PAGEDOWN:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Next;
                    break;
                case TermKeySym.TERMKEY_SYM_SPACE:
                    inputRecord.KeyEvent.UnicodeChar = ' ';
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Space;
                    break;
                case TermKeySym.TERMKEY_SYM_ESCAPE:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Escape;
                    break;
                case TermKeySym.TERMKEY_SYM_INSERT:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Insert;
                    break;
                case TermKeySym.TERMKEY_SYM_UP:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Up;
                    break;
                case TermKeySym.TERMKEY_SYM_DOWN:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Down;
                    break;
                case TermKeySym.TERMKEY_SYM_LEFT:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Left;
                    break;
                case TermKeySym.TERMKEY_SYM_RIGHT:
                    inputRecord.KeyEvent.wVirtualKeyCode = VirtualKeys.Right;
                    break;
                default:
                    throw new NotSupportedException("Not supported keyboard code detected: " + key.code.sym);
            }
            inputRecord.KeyEvent.dwControlKeyState = 0;
            if ((key.modifiers & 4) == 4)
            {
                inputRecord.KeyEvent.dwControlKeyState |= ControlKeyState.LEFT_CTRL_PRESSED;
            }
            if ((key.modifiers & 2) == 2)
            {
                inputRecord.KeyEvent.dwControlKeyState |= ControlKeyState.LEFT_ALT_PRESSED;
            }
            processInputEvent(inputRecord);
        }
        else if (key.type == TermKeyType.TERMKEY_TYPE_UNICODE)
        {
            byte[] data =
            [
                key.utf8_0,
                key.utf8_1,
                key.utf8_2,
                key.utf8_3,
                key.utf8_4,
                key.utf8_5,
                key.utf8_6,
            ];
            string d = System.Text.Encoding.UTF8.GetString(data);
            char unicodeCharacter = d[0];
            INPUT_RECORD inputRecord = new()
            {
                EventType = EventType.KEY_EVENT
            };
            inputRecord.KeyEvent.bKeyDown = true;
            inputRecord.KeyEvent.wRepeatCount = 1;
            inputRecord.KeyEvent.UnicodeChar = unicodeCharacter;
            inputRecord.KeyEvent.dwControlKeyState = 0;
            if (char.IsLetterOrDigit(unicodeCharacter))
            {
                if (char.IsDigit(unicodeCharacter))
                {
                    inputRecord.KeyEvent.wVirtualKeyCode =
                        (VirtualKeys)(unicodeCharacter - '0' + (int)VirtualKeys.N0);
                }
                else
                {
                    char lowercased = char.ToLowerInvariant(unicodeCharacter);

                    // Only english characters can be converted to VirtualKeys
                    if (lowercased >= 'a' && lowercased <= 'z')
                    {
                        inputRecord.KeyEvent.wVirtualKeyCode =
                            (VirtualKeys)(lowercased - 'a' + (int)VirtualKeys.A);
                    }
                }
            }
            if ((key.modifiers & 4) == 4)
            {
                inputRecord.KeyEvent.dwControlKeyState |= ControlKeyState.LEFT_CTRL_PRESSED;
            }
            if ((key.modifiers & 2) == 2)
            {
                inputRecord.KeyEvent.dwControlKeyState |= ControlKeyState.LEFT_ALT_PRESSED;
            }
            // todo : remove hardcoded exit combo after testing
            if (unicodeCharacter == 'd' && key.modifiers == 4)
            {
                Exit();
            }
            processInputEvent(inputRecord);
            //
        }
        else if (key.type == TermKeyType.TERMKEY_TYPE_MOUSE)
        {
            LibTermKey.termkey_interpret_mouse(termkeyHandle, ref key, out TermKeyMouseEvent ev, out int button, out int line, out int col);
            //
            INPUT_RECORD inputRecord = new()
            {
                EventType = EventType.MOUSE_EVENT
            };
            if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_PRESS || ev == TermKeyMouseEvent.TERMKEY_MOUSE_RELEASE)
                inputRecord.MouseEvent.dwEventFlags = MouseEventFlags.PRESSED_OR_RELEASED;
            if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_DRAG)
                inputRecord.MouseEvent.dwEventFlags = MouseEventFlags.MOUSE_MOVED;
            inputRecord.MouseEvent.dwMousePosition = new COORD((short)(col - 1), (short)(line - 1));
            if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_RELEASE)
            {
                inputRecord.MouseEvent.dwButtonState = 0;
            }
            else if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_DRAG || ev == TermKeyMouseEvent.TERMKEY_MOUSE_PRESS)
            {
                if (1 == button)
                {
                    inputRecord.MouseEvent.dwButtonState = MOUSE_BUTTON_STATE.FROM_LEFT_1ST_BUTTON_PRESSED;
                }
                else if (2 == button)
                {
                    inputRecord.MouseEvent.dwButtonState = MOUSE_BUTTON_STATE.FROM_LEFT_2ND_BUTTON_PRESSED;
                }
                else if (3 == button)
                {
                    inputRecord.MouseEvent.dwButtonState = MOUSE_BUTTON_STATE.RIGHTMOST_BUTTON_PRESSED;
                }
            }
            //
            processInputEvent(inputRecord);
        }
    }

    private void runWindows(Control control)
    {
        mainControl = control;
        //
        stdInputHandle = Win32.GetStdHandle(StdHandleType.STD_INPUT_HANDLE);
        stdOutputHandle = Win32.GetStdHandle(StdHandleType.STD_OUTPUT_HANDLE);
        IntPtr[] handles = [
                exitWaitHandle.GetSafeWaitHandle().DangerousGetHandle(),
                stdInputHandle,
                invokeWaitHandle.GetSafeWaitHandle().DangerousGetHandle(  )
            ];

        // Set console mode to enable mouse and window resizing events
        const uint ENABLE_WINDOW_INPUT = 0x0008;
        const uint ENABLE_MOUSE_INPUT = 0x0010;
        const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        Win32.GetConsoleMode(stdInputHandle, out uint consoleMode);
        Win32.SetConsoleMode(stdInputHandle,
            (consoleMode | ENABLE_MOUSE_INPUT | ENABLE_WINDOW_INPUT | ENABLE_EXTENDED_FLAGS)
            & ~ENABLE_QUICK_EDIT_MODE);

        // Get console screen buffer size
        Win32.GetConsoleScreenBufferInfo(stdOutputHandle, out CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo);

        // Set Canvas size to current console window size (not to whole buffer size)
        savedWindowRect = new Rect(new Point(Console.WindowLeft, Console.WindowTop),
                                    new Size(Console.WindowWidth, Console.WindowHeight));

        canvas = userCanvasSize.IsEmpty
            ? new PhysicalCanvas(savedWindowRect.Width, savedWindowRect.Height, stdOutputHandle)
            : new PhysicalCanvas(userCanvasSize.Width, userCanvasSize.Height, stdOutputHandle);
        renderer.Canvas = canvas;

        // Fill the canvas by default
        renderer.RootElementRect = userRootElementRect.IsEmpty
            ? new Rect(new Point(0, 0), canvas.Size) : userRootElementRect;
        renderer.RootElement = mainControl;
        //
        mainControl.Invalidate();
        renderer.UpdateLayout();
        renderer.FinallyApplyChangesToCanvas();

        // Initially hide the console cursor
        HideCursor();

        int previousWindowWidth = Console.WindowWidth;
        int previousWindowHeight = Console.WindowHeight;
        int previousWindowLeft = Console.WindowLeft;
        int previousWindowTop = Console.WindowTop;

        running = true;
        mainThreadId = Environment.CurrentManagedThreadId;
        //
        while (true)
        {
            // 100 ms instead of Win32.INFINITE to check console window Zoomed and Iconic
            // state periodically (because if user presses Maximize/Restore button
            // there are no input event generated).
            uint waitResult = Win32.WaitForMultipleObjects(3, handles, false, 100);
            if (waitResult == 0)
            {
                break;
            }
            if (waitResult == 1)
            {
                processInput();
            }
            if (waitResult == 2)
            {
                // Do nothing special - because invokeActions will be invoked in loop anyway
            }

            // If we received WAIT_TIMEOUT - check window Zoomed and Iconic state
            // and correct buffer size and console window size
            if (waitResult == 0x00000102)
            {
                IntPtr consoleWindow = getConsoleWindowHwnd();
                bool isZoomed = Win32.IsZoomed(consoleWindow);
                bool isIconic = Win32.IsIconic(consoleWindow);
                if (maximized != isZoomed && !isIconic)
                {
                    if (isZoomed)
                        Maximize();
                    else
                        Restore();
                }
                if (!maximized)
                {
                    savedWindowRect = new Rect(new Point(Console.WindowLeft, Console.WindowTop),
                                                new Size(Console.WindowWidth, Console.WindowHeight));
                }
            }
            // WAIT_FAILED
            if (waitResult == 0xFFFFFFFF)
            {
                throw new InvalidOperationException("Invalid wait result of WaitForMultipleObjects.");
            }

            int currentWindowWidth = Console.WindowWidth;
            int currentWindowHeight = Console.WindowHeight;
            int currentWindowLeft = Console.WindowLeft;
            int currentWindowTop = Console.WindowTop;

            bool windowSizeChanged = currentWindowWidth != previousWindowWidth || currentWindowHeight != previousWindowHeight;
            bool windowBoundsChanged = windowSizeChanged || currentWindowLeft != previousWindowLeft || currentWindowTop != previousWindowTop;

            if (windowBoundsChanged && !maximized)
            {
                savedWindowRect = new Rect(new Point(currentWindowLeft, currentWindowTop),
                                            new Size(currentWindowWidth, currentWindowHeight));
            }

            if (windowSizeChanged)
            {
                emitWindowsTerminalSizeChangedEvent(currentWindowWidth, currentWindowHeight);
            }

            previousWindowWidth = currentWindowWidth;
            previousWindowHeight = currentWindowHeight;
            previousWindowLeft = currentWindowLeft;
            previousWindowTop = currentWindowTop;

            while (true)
            {
                bool anyInvokeActions = isAnyInvokeActions();
                bool anyRoutedEvent = !EventManager.IsQueueEmpty();
                bool anyLayoutToRevalidate = renderer.AnyControlInvalidated;

                if (!anyInvokeActions && !anyRoutedEvent && !anyLayoutToRevalidate)
                    break;

                EventManager.ProcessEvents();
                processInvokeActions();
                renderer.UpdateLayout();
            }

            renderer.FinallyApplyChangesToCanvas();
        }

        // Restore cursor visibility before exit
        ShowCursor();

        // Restore console mode before exit
        Win32.SetConsoleMode(stdInputHandle, consoleMode);

        renderer.RootElement = null;

        // todo : restore attributes of console output
    }

    private bool isAnyInvokeActions()
    {
        lock (actionsLocker)
        {
            return (actionsToBeInvoked.Count != 0);
        }
    }

    private void processInvokeActions()
    {
        for (; ; )
        {
            ActionInfo top;
            lock (actionsLocker)
            {
                if (actionsToBeInvoked.Count != 0)
                {
                    top = actionsToBeInvoked[0];
                    actionsToBeInvoked.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
            top.action.Invoke();
            top.waitHandle?.Set();
        }
    }

    private void processInput()
    {
        INPUT_RECORD[] buffer = new INPUT_RECORD[10];
        bool bReaded = Win32.ReadConsoleInput(stdInputHandle, buffer, (uint)buffer.Length, out uint read);
        if (!bReaded)
        {
            throw new InvalidOperationException("ReadConsoleInput method failed.");
        }

        for (int i = 0; i < read; ++i)
        {
            processInputEvent(buffer[i]);
        }
    }

    private void processInputEvent(INPUT_RECORD inputRecord)
    {
        if (inputRecord.EventType == EventType.WINDOW_BUFFER_SIZE_EVENT)
        {

            if (usingLinux)
            {
                // Reinitializing ncurses to deal with new dimensions
                // http://stackoverflow.com/questions/13707137/ncurses-resizing-glitch
                NCurses.endwin();
                // Needs to be called after an endwin() so ncurses will initialize
                // itself with the new terminal dimensions.
                NCurses.refresh();
                NCurses.clear();
            }

            COORD dwSize = inputRecord.WindowBufferSizeEvent.dwSize;

            // Invoke default handler if no custom handler is attached.
            // This keeps canvas/root bounds in sync with current terminal size.
            if (TerminalSizeChanged == null)
            {
                OnTerminalSizeChangedDefault(this, new TerminalSizeChangedEventArgs(dwSize.X, dwSize.Y));
            }
            else
            {
                TerminalSizeChanged?.Invoke(this, new TerminalSizeChangedEventArgs(dwSize.X, dwSize.Y));
            }

            // Refresh whole display
            renderer.FinallyApplyChangesToCanvas(true);

            return;
        }
        eventManager.ParseInputEvent(inputRecord, mainControl);
    }

    /// <summary>
    /// Checks if current thread is same thread from which Run() method
    /// was called.
    /// </summary>
    /// <returns></returns>
    public bool IsUiThread()
    {
        return Environment.CurrentManagedThreadId == mainThreadId;
    }

    /// <summary>
    /// Invokes action in UI thread synchronously.
    /// If run loop was not started yet, nothing will be done.
    /// </summary>
    /// <param name="action"></param>
    public void RunOnUiThread(Action action)
    {
        // If run loop is not started, do nothing
        if (!running)
        {
            return;
        }
        // If current thread is UI thread, invoke action directly
        if (IsUiThread())
        {
            action.Invoke();
            return;
        }
        using EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset);
        lock (actionsLocker)
        {
            actionsToBeInvoked.Add(new ActionInfo(action, waitHandle));
        }
        if (usingLinux)
        {
            Libc.writeInt64(pipeFds[1], 3);
        }
        else
        {
            invokeWaitHandle.Set();
        }

        waitHandle.WaitOne();
    }

    /// <summary>
    /// Invokes action in main loop thread asynchronously.
    /// If run loop was not started yet, nothing will be done.
    /// </summary>
    public void Post(Action action)
    {
        // If run loop is not started, nothing to do
        if (!running)
        {
            return;
        }
        lock (actionsLocker)
        {
            actionsToBeInvoked.Add(new ActionInfo(action, null));
        }
        if (!IsUiThread())
        {
            if (usingLinux)
            {
                Libc.writeInt64(pipeFds[1], 3);
            }
            else
            {
                invokeWaitHandle.Set();
            }
        }
    }

    private readonly Lock timersLock = new();

    /// <summary>
    /// This structure is required to avoid active timer to be collected by GC
    /// before action execution.
    /// </summary>
    private readonly List<Timer> activeTimers = [];

    /// <summary>
    /// Invokes action in main loop thread (UI thread) asynchronously and after delay.
    /// If run loop will not start to delayed time, nothing will be done.
    /// </summary>
    public void Post(Action action, TimeSpan delay)
    {
        lock (timersLock)
        {
            Timer[] array = new Timer[1];
            Timer timer = new(state =>
            {
                Post(action);
                lock (timersLock)
                {
                    activeTimers.Remove(array[0]);
                }
            }, null, delay, TimeSpan.FromMilliseconds(-1));
            array[0] = timer;
            activeTimers.Add(timer);
        }
    }

    /// <summary>
    /// Begins capturing mouse and routed events
    /// by the specified control element. After this, the control receives all mouse events
    /// as the event source (regardless of the mouse cursor position), and all routed
    /// events are delivered only to this control and its children.
    /// Used, for example, when processing a button click - after pressing, input is captured, and
    /// events come only to the button. When the user releases the mouse button, capture is released.
    /// </summary>
    public void BeginCaptureInput(Control control)
    {
        eventManager.BeginCaptureInput(control);
    }

    /// <summary>
    /// Ends capturing mouse and routed events.
    /// </summary>
    public void EndCaptureInput(Control control)
    {
        eventManager.EndCaptureInput(control);
    }

    private void dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            exitWaitHandle?.Dispose();
        }
    }

    public void Dispose()
    {
        dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ConsoleApplication()
    {
        dispose(false);
    }
}

