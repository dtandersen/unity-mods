using Sage.Entity;

namespace Sage.Repository;

/// <summary>In-memory structure inspector. Used by tests.</summary>
public sealed class InMemoryStructureInspector : IStructureInspector
{
    public StructureDetails? Details { get; set; }

    public string? LastStructure { get; private set; }

    public StructureDetails? Describe(string? structure)
    {
        LastStructure = structure;
        return Details;
    }
}
