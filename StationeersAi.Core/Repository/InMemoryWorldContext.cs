using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory world context. Used by tests.</summary>
public sealed class InMemoryWorldContext : IWorldContext
{
    public WorldInfo? Info { get; set; }
}
