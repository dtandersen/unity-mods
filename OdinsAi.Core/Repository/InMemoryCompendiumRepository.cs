using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory compendium. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryCompendiumRepository : ICompendiumRepository
    {
        private readonly List<CompendiumEntry> _entries = new List<CompendiumEntry>();

        public void Add(CompendiumEntry entry)
        {
            _entries.Add(entry);
        }

        public IReadOnlyList<CompendiumEntry> GetEntries()
        {
            return _entries;
        }
    }
}