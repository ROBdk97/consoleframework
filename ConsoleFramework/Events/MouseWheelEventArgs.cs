using ConsoleFramework.Core;

namespace ConsoleFramework.Events
{
    public class MouseWheelEventArgs(object source, RoutedEvent routedEvent, Point rawPosition,
        MouseButtonState leftButton, MouseButtonState middleButton,
        MouseButtonState rightButton, int delta) : MouseEventArgs(source, routedEvent, rawPosition, leftButton, middleButton, rightButton)
    {
        public int Delta { get; } = delta;
    }
}
