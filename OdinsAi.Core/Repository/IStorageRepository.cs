using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Read-only access to the containers near the player.</summary>
    public interface IStorageRepository
    {
        IReadOnlyList<StorageContainer> GetNearbyContainers(float radius);
    }
}