using System;

namespace ConsoleFramework.Events
{

    /// <summary>Key for internal usage in routed event management maps.</summary>
    public sealed class RoutedEventKey : IEquatable<RoutedEventKey>
    {
        private readonly string name;
        private readonly Type ownerType;

        public string Name => name;
        public Type OwnerType => ownerType;

        public RoutedEventKey(string name, Type ownerType)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(ownerType);

            this.name = name;
            this.ownerType = ownerType;
        }

        public bool Equals(RoutedEventKey? other) =>
            other is not null && Equals(other.name, name) && Equals(other.ownerType, ownerType);

        public override bool Equals(object? obj) =>
            obj is RoutedEventKey key && Equals(key);

        public override int GetHashCode() => HashCode.Combine(name, ownerType);

        public static bool operator ==(RoutedEventKey left, RoutedEventKey right) => Equals(left, right);
        public static bool operator !=(RoutedEventKey left, RoutedEventKey right) => !Equals(left, right);
    }
}
