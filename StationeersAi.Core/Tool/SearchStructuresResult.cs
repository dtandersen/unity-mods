using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>Live structures matching a search.</summary>
public sealed record SearchStructuresResult(IReadOnlyList<StructureInfo> Structures);
