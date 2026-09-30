using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Query
{
    /// <summary>
    /// Search over structures. Matching is case-insensitive substring on the
    /// structure name; order is not significant.
    /// </summary>
    public sealed class StructureIndex
    {
        private readonly IStructureRepository _structures;

        public StructureIndex(IStructureRepository structures)
        {
            _structures = structures;
        }

        public IReadOnlyList<Structure> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<Structure>();
            }

            return _structures.GetAll()
                .Where(structure => structure.Name != null &&
                    structure.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
        }
    }
}
