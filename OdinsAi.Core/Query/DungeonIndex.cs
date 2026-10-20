using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>
    /// Search over nearby dungeons. Optional name filter; results are nearest first.
    /// </summary>
    public sealed class DungeonIndex
    {
        private readonly IDungeonRepository _dungeons;

        public DungeonIndex(IDungeonRepository dungeons)
        {
            _dungeons = dungeons;
        }

        public IReadOnlyList<Dungeon> Search(float radius, string query)
        {
            var nearby = _dungeons.GetNearbyDungeons(radius);

            var matches = string.IsNullOrWhiteSpace(query)
                ? nearby
                : nearby.Where(dungeon => dungeon.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);

            return matches.OrderBy(dungeon => dungeon.Distance).ToArray();
        }
    }
}