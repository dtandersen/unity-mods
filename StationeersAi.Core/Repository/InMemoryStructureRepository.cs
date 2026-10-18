using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory structure repository. Used by tests.</summary>
public sealed class InMemoryStructureRepository : IStructureRepository
{
    private readonly List<Structure> _structures;

    public InMemoryStructureRepository(IEnumerable<Structure> structures)
    {
        _structures = (structures ?? throw new ArgumentNullException(nameof(structures))).ToList();
    }

    public void Add(Structure structure)
    {
        _structures.Add(structure);
    }

    public void Update(Structure structure)
    {
        _structures.RemoveAll(existing => existing.Id == structure.Id);
        _structures.Add(structure);
    }

    public IReadOnlyList<Structure> GetAll()
    {
        return _structures;
    }

    public Structure? GetById(string id)
    {
        return _structures.FirstOrDefault(structure => structure.Id == id);
    }
}
