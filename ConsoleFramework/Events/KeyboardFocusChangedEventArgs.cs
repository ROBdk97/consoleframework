using ConsoleFramework.Controls;

namespace ConsoleFramework.Events;

public class KeyboardFocusChangedEventArgs : RoutedEventArgs
{
    public KeyboardFocusChangedEventArgs(object source, RoutedEvent routedEvent)
        : base(source, routedEvent) { }

    public KeyboardFocusChangedEventArgs(object source, RoutedEvent routedEvent,
        Control oldFocus, Control newFocus)
        : base(source, routedEvent)
    {
        OldFocus = oldFocus;
        NewFocus = newFocus;
    }

    public Control? OldFocus { get; }
    public Control? NewFocus { get; }
}