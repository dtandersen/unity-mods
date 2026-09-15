using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>
    /// A <see cref="IPlayerContext"/> backed by a live Valheim <see cref="Player"/>.
    /// The context owns the player-specific readers; the rest of the mod only sees
    /// the Unity-free interface.
    /// </summary>
    public sealed class ValheimPlayerContext : IPlayerContext
    {
        private readonly Player _player;
        private readonly ValheimInventoryRepository _inventory;
        private readonly ValheimLocationRepository _location;
        private readonly ValheimStorageRepository _storage;
        private readonly ValheimCompendiumRepository _compendium;

        /// <param name="player">The player to read. May be null.</param>
        /// <param name="includeUnseen">Include compendium pages the player has not uncovered.</param>
        public ValheimPlayerContext(Player player, bool includeUnseen)
        {
            _player = player;
            _inventory = new ValheimInventoryRepository(player);
            _location = new ValheimLocationRepository(player);
            _storage = new ValheimStorageRepository(player);
            _compendium = new ValheimCompendiumRepository(player, includeUnseen);
        }

        public string Name => _player != null ? _player.GetPlayerName() : null;

        public OdinsAi.Entity.Location Location => _location.GetLocation();

        public IReadOnlyList<ItemStack> Inventory => _inventory.GetCarriedItems();

        public IReadOnlyList<CompendiumEntry> Compendium => _compendium.GetEntries();

        public IReadOnlyList<StorageContainer> GetNearbyStorage(float radius)
        {
            return _storage.GetNearbyContainers(radius);
        }
    }
}