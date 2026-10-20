using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: look up the player's current location.</summary>
    public sealed class GetLocationCommand
    {
        private readonly IPlayerContext _player;

        public GetLocationCommand(IPlayerContext player)
        {
            _player = player;
        }

        public Location Execute()
        {
            return _player.Location;
        }
    }
}