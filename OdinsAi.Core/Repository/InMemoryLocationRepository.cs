using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory location. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryLocationRepository : ILocationRepository
    {
        public Location Current { get; set; }

        public Location GetLocation()
        {
            return Current;
        }
    }
}