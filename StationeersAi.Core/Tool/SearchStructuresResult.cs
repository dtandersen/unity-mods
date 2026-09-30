using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Structures matching a search.</summary>
    public sealed record SearchStructuresResult(IReadOnlyList<Structure> Structures);
}
