using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory resource nodes. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryResourceRepository : IResourceRepository
    {
        private readonly List<ResourceNode> _resources = new List<ResourceNode>();

        public void Add(ResourceNode resource)
        {
            _resources.Add(resource);
        }

        public IReadOnlyList<ResourceNode> GetNearbyResources(float radius)
        {
            return _resources.Where(resource => resource.Distance <= radius).ToArray();
        }
    }
}