using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: look up the player's current location.</summary>
    public sealed class GetLocationCommand
    {
        private readonly ILocationRepository _locations;

        public GetLocationCommand(ILocationRepository locations)
        {
            _locations = locations;
        }

        public Location Execute()
        {
            return _locations.GetLocation();
        }
    }
}