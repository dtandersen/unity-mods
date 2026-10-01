using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command
{
    /// <summary>Domain operation: report the world.</summary>
    public sealed class GetWorldCommand
    {
        private readonly IWorldContext _world;

        public GetWorldCommand(IWorldContext world)
        {
            _world = world;
        }

        public WorldInfo Execute()
        {
            return _world.Info;
        }
    }
}
