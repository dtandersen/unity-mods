using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Read-only access to creatures near the player.</summary>
    public interface ICreatureRepository
    {
        IReadOnlyList<Creature> GetNearbyCreatures(float radius);
    }
}