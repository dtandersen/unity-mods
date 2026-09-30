using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command
{
    /// <summary>Domain operation: find structures by name.</summary>
    public sealed class SearchStructuresCommand
    {
        private readonly StructureIndex _structures;

        public SearchStructuresCommand(StructureIndex structures)
        {
            _structures = structures;
        }

        public IReadOnlyList<Structure> Execute(string query)
        {
            return _structures.Search(query);
        }
    }
}
