namespace ConsoleFramework.Events;

public class CancelEventArgs : RoutedEventArgs
{
    public CancelEventArgs(object source, RoutedEvent routedEvent) : base(source, routedEvent) { }

    public bool Cancel { get; set; }
}
