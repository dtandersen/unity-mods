using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>
    /// Search over nearby resource nodes. Optional name/type filter; nearest first.
    /// </summary>
    public sealed class ResourceIndex
    {
        private readonly IResourceRepository _resources;

        public ResourceIndex(IResourceRepository resources)
        {
            _resources = resources;
        }

        public IReadOnlyList<ResourceNode> Search(float radius, string query)
        {
            var nearby = _resources.GetNearbyResources(radius);

            var matches = string.IsNullOrWhiteSpace(query)
                ? nearby
                : nearby.Where(resource =>
                    resource.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    resource.Type.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);

            return matches.OrderBy(resource => resource.Distance).ToArray();
        }
    }
}