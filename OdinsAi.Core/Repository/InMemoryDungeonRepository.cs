using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory dungeons. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryDungeonRepository : IDungeonRepository
    {
        private readonly List<Dungeon> _dungeons = new List<Dungeon>();

        public void Add(Dungeon dungeon)
        {
            _dungeons.Add(dungeon);
        }

        public IReadOnlyList<Dungeon> GetNearbyDungeons(float radius)
        {
            return _dungeons.Where(dungeon => dungeon.Distance <= radius).ToArray();
        }
    }
}