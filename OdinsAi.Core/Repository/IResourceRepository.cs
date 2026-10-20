using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Read-only access to resource nodes near the player.</summary>
    public interface IResourceRepository
    {
        IReadOnlyList<ResourceNode> GetNearbyResources(float radius);
    }
}