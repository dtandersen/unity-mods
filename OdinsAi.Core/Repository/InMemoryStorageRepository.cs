using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory storage. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryStorageRepository : IStorageRepository
    {
        private readonly List<StorageContainer> _containers = new List<StorageContainer>();

        public void Add(StorageContainer container)
        {
            _containers.Add(container);
        }

        public IReadOnlyList<StorageContainer> GetNearbyContainers(float radius)
        {
            return _containers;
        }
    }
}