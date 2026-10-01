using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>The world's setup, orbit, sunlight, and outside air.</summary>
    public sealed record GetWorldResult(WorldInfo Info);
}
