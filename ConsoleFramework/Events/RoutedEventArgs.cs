using System;

namespace ConsoleFramework.Events;

public class RoutedEventArgs(object source, RoutedEvent routedEvent) : EventArgs
{
    public bool Handled { get; set; }
    public object Source { get; } = source;
    public RoutedEvent RoutedEvent { get; } = routedEvent;
}