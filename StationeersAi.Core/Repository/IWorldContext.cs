using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>The world as seen by the tools. Built per request.</summary>
public interface IWorldContext
{
    /// <summary>The world info, or null when unavailable.</summary>
    WorldInfo? Info { get; }
}
