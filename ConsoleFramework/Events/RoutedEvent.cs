using System;

namespace ConsoleFramework.Events;

/// <summary>Represents an event that supports routing through the visual tree.</summary>
public sealed class RoutedEvent(Type handlerType, string name, Type ownerType, RoutingStrategy routingStrategy)
{
    public Type HandlerType { get; } = handlerType;
    public string Name { get; } = name;
    public Type OwnerType { get; } = ownerType;
    public RoutingStrategy RoutingStrategy { get; } = routingStrategy;
    public RoutedEventKey Key => new(Name, OwnerType);
}
