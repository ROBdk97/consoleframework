using System;

namespace ConsoleFramework.Events;

/// <summary>Routing strategy for an event.</summary>
public enum RoutingStrategy
{
    /// <summary>Event travels from root to source.</summary>
    Tunnel,
    /// <summary>Event travels from source to root.</summary>
    Bubble,
    /// <summary>Event is delivered only to direct subscribers on the source.</summary>
    Direct
}

/// <summary>Key for internal usage in routed event management maps.</summary>
public sealed class RoutedEventKey : IEquatable<RoutedEventKey>
{
    private readonly string name;
    private readonly Type ownerType;

    public string Name => name;
    public Type OwnerType => ownerType;

    public RoutedEventKey(string name, Type ownerType)
    {
        this.name = name;
        this.ownerType = ownerType;
    }

    public bool Equals(RoutedEventKey? other) =>
        other is not null && Equals(other.name, name) && Equals(other.ownerType, ownerType);

    public override bool Equals(object? obj) =>
        obj is RoutedEventKey key && Equals(key);

    public override int GetHashCode() => HashCode.Combine(name, ownerType);

    public static bool operator ==(RoutedEventKey? left, RoutedEventKey? right) => Equals(left, right);
    public static bool operator !=(RoutedEventKey? left, RoutedEventKey? right) => !Equals(left, right);
}

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
