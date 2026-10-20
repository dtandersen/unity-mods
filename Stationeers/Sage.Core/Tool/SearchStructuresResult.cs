using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Tool;

/// <summary>Live structures matching a search.</summary>
public sealed record SearchStructuresResult(IReadOnlyList<StructureInfo> Structures);
