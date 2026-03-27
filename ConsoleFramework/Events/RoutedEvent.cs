using System;

namespace ConsoleFramework.Events;

/// <summary>Represents an event that supports routing through the visual tree.</summary>
public sealed class RoutedEvent
{
    public Type HandlerType { get; }
    public string Name { get; }
    public Type OwnerType { get; }
    public RoutingStrategy RoutingStrategy { get; }
    public RoutedEventKey Key => new(Name, OwnerType);

    public RoutedEvent(Type handlerType, string name, Type ownerType, RoutingStrategy routingStrategy)
    {
        HandlerType = handlerType;
        Name = name;
        OwnerType = ownerType;
        RoutingStrategy = routingStrategy;
    }
}
