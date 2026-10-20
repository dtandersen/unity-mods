using Sage.Entity;

namespace Sage.Repository;

/// <summary>Full details of one live structure.</summary>
public interface IStructureInspector
{
    /// <summary>Details for the named structure, or null when no single structure matches.</summary>
    StructureDetails? Describe(string? structure);
}
