using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>Read-only access to the structure "database".</summary>
public interface IStructureRepository
{
    /// <summary>All known structures. Order is not significant.</summary>
    IReadOnlyList<Structure> GetAll();

    /// <summary>The structure with the given id, or null if none exists.</summary>
    Structure? GetById(string id);
}
