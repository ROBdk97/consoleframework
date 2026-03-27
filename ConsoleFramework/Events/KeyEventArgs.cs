using ConsoleFramework.Native;

namespace ConsoleFramework.Events;

public class KeyEventArgs(object source, RoutedEvent routedEvent) : RoutedEventArgs(source, routedEvent)
{
    public bool bKeyDown;
    public ushort wRepeatCount;
    public VirtualKeys wVirtualKeyCode;
    public ushort wVirtualScanCode;
    public char UnicodeChar;
    public ControlKeyState dwControlKeyState;
}