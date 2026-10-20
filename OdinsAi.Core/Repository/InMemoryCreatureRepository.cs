using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory creatures. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryCreatureRepository : ICreatureRepository
    {
        private readonly List<Creature> _creatures = new List<Creature>();

        public void Add(Creature creature)
        {
            _creatures.Add(creature);
        }

        public IReadOnlyList<Creature> GetNearbyCreatures(float radius)
        {
            return _creatures.Where(creature => creature.Distance <= radius).ToArray();
        }
    }
}