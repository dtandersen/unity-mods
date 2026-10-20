using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>Search over nearby creatures. Optional name filter; nearest first.</summary>
    public sealed class CreatureIndex
    {
        private readonly ICreatureRepository _creatures;

        public CreatureIndex(ICreatureRepository creatures)
        {
            _creatures = creatures;
        }

        public IReadOnlyList<Creature> Search(float radius, string query)
        {
            var nearby = _creatures.GetNearbyCreatures(radius);

            var matches = string.IsNullOrWhiteSpace(query)
                ? nearby
                : nearby.Where(creature => creature.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);

            return matches.OrderBy(creature => creature.Distance).ToArray();
        }
    }
}