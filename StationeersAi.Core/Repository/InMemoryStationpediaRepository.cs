using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory Stationpedia repository. Used by tests.</summary>
    public sealed class InMemoryStationpediaRepository : IStationpediaRepository
    {
        private readonly List<StationpediaEntry> _entries;

        public InMemoryStationpediaRepository(IEnumerable<StationpediaEntry> entries = null)
        {
            _entries = entries?.ToList() ?? new List<StationpediaEntry>();
        }

        public void Add(StationpediaEntry entry)
        {
            _entries.Add(entry);
        }

        public IReadOnlyList<StationpediaEntry> GetAll()
        {
            return _entries;
        }

        public StationpediaEntry GetByKey(string key)
        {
            return _entries.FirstOrDefault(entry => entry.Key == key);
        }
    }
}
