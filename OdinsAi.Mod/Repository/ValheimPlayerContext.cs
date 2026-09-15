using System;
using System.Collections.Generic;
using OdinsAi.Entity;
using UnityEngine;

namespace OdinsAi.Repository
{
    /// <summary>
    /// A <see cref="IPlayerContext"/> backed by a live Valheim <see cref="Player"/>.
    /// The context owns the player-specific readers and snapshots the player's
    /// known recipes and nearby stations on construction, so the rest of the mod can
    /// use it off the main thread.
    /// </summary>
    public sealed class ValheimPlayerContext : IPlayerContext
    {
        private readonly Player _player;
        private readonly ValheimInventoryRepository _inventory;
        private readonly ValheimLocationRepository _location;
        private readonly ValheimStorageRepository _storage;
        private readonly ValheimCompendiumRepository _compendium;
        private readonly HashSet<string> _knownRecipes;
        private readonly List<StationInfo> _stations;

        /// <param name="database">The game database, used to map known recipe keys to item ids.</param>
        /// <param name="player">The player to read. May be null.</param>
        /// <param name="includeUnseen">Include compendium pages the player has not uncovered.</param>
        public ValheimPlayerContext(ObjectDB database, Player player, bool includeUnseen)
        {
            _player = player;
            _inventory = new ValheimInventoryRepository(player);
            _location = new ValheimLocationRepository(player);
            _storage = new ValheimStorageRepository(player);
            _compendium = new ValheimCompendiumRepository(player, includeUnseen);
            _knownRecipes = KnownRecipes(database, player);
            _stations = ReadAvailableStations(player);
        }

        public string Name => _player != null ? _player.GetPlayerName() : null;

        public OdinsAi.Entity.Location Location => _location.GetLocation();

        public IReadOnlyList<ItemStack> Inventory => _inventory.GetCarriedItems();

        public IReadOnlyList<CompendiumEntry> Compendium => _compendium.GetEntries();

        public IReadOnlyList<StationInfo> AvailableStations => _stations;

        public IReadOnlyList<StorageContainer> GetNearbyStorage(float radius)
        {
            return _storage.GetNearbyContainers(radius);
        }

        public bool IsRecipeKnown(string recipeId)
        {
            return recipeId != null && _knownRecipes.Contains(recipeId);
        }

        private static HashSet<string> KnownRecipes(ObjectDB database, Player player)
        {
            var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (database == null || database.m_recipes == null || player == null)
            {
                return known;
            }

            foreach (var recipe in database.m_recipes)
            {
                if (recipe == null || recipe.m_item == null)
                {
                    continue;
                }

                var shared = recipe.m_item.m_itemData != null ? recipe.m_item.m_itemData.m_shared : null;
                if (shared != null && player.IsRecipeKnown(shared.m_name))
                {
                    known.Add(recipe.m_item.name);
                }
            }

            return known;
        }

        private static List<StationInfo> ReadAvailableStations(Player player)
        {
            var stations = new List<StationInfo>();
            if (player == null || CraftingStation.Instances == null)
            {
                return stations;
            }

            var origin = player.transform.position;
            var best = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var instance in CraftingStation.Instances)
            {
                if (!(instance is CraftingStation station) || station == null)
                {
                    continue;
                }

                // Match HaveBuildStationInRange: ignore the vertical difference.
                var stationPosition = station.transform.position;
                var target = new Vector3(origin.x, stationPosition.y, origin.z);
                if (Vector3.Distance(stationPosition, target) > station.GetStationBuildRange())
                {
                    continue;
                }

                var name = ValheimItemRepository.Localize(station.m_name);
                var level = station.GetLevel();
                if (!best.TryGetValue(name, out var existing) || level > existing)
                {
                    best[name] = level;
                }
            }

            foreach (var pair in best)
            {
                stations.Add(new StationInfo(pair.Key, pair.Value));
            }

            return stations;
        }
    }
}