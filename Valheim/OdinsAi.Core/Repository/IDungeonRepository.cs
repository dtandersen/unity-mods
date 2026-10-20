using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Read-only access to dungeon entrances near the player.</summary>
    public interface IDungeonRepository
    {
        IReadOnlyList<Dungeon> GetNearbyDungeons(float radius);
    }
}