using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Reads the target player's position and current biome from the live game.</summary>
    public sealed class ValheimLocationRepository : ILocationRepository
    {
        private readonly Player _player;

        /// <param name="player">The player to read.</param>
        public ValheimLocationRepository(Player player)
        {
            _player = player;
        }

        public OdinsAi.Entity.Location GetLocation()
        {
            if (_player == null)
            {
                return null;
            }

            var position = _player.transform.position;
            return new OdinsAi.Entity.Location(position.x, position.y, position.z, BiomeName(_player));
        }

        private static string BiomeName(Player player)
        {
            // GetCurrentBiomeData() applies alt-biome name overrides and localizes.
            var data = player.GetCurrentBiomeData();
            if (data != null)
            {
                var name = data.GetName();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }

            return ValheimItemRepository.Localize(BiomeSector.GetBiomeName(player.GetCurrentBiome()));
        }
    }
}