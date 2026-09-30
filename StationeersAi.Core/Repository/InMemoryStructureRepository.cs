using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory structure repository. Used by tests.</summary>
    public sealed class InMemoryStructureRepository : IStructureRepository
    {
        private readonly List<Structure> _structures;

        public InMemoryStructureRepository(IEnumerable<Structure> structures = null)
        {
            _structures = structures?.ToList() ?? new List<Structure>();
        }

        public void Add(Structure structure)
        {
            _structures.Add(structure);
        }

        public IReadOnlyList<Structure> GetAll()
        {
            return _structures;
        }

        public Structure GetById(string id)
        {
            return _structures.FirstOrDefault(structure => structure.Id == id);
        }
    }
}
