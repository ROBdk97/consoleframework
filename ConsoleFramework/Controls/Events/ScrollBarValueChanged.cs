using ConsoleFramework.Events;

namespace ConsoleFramework.Controls.Events
{
    public class ScrollBarValueChanged(object source, RoutedEvent routedEvent) : RoutedEventArgs(source, routedEvent)
    {
    }
}

