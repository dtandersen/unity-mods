using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Reads the player's position and current biome from the live game.</summary>
    public sealed class ValheimLocationRepository : ILocationRepository
    {
        public OdinsAi.Entity.Location GetLocation()
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return null;
            }

            var position = player.transform.position;
            return new OdinsAi.Entity.Location(position.x, position.y, position.z, BiomeName(player));
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