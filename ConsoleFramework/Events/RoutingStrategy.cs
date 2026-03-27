namespace ConsoleFramework.Events
{

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
}
