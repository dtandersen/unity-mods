using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Read-only access to the player's current location.</summary>
    public interface ILocationRepository
    {
        /// <summary>The player's location, or null when unavailable.</summary>
        Location GetLocation();
    }
}