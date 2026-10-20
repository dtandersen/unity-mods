using System;
using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory player context. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryPlayerContext : IPlayerContext
    {
        private readonly List<ItemStack> _inventory = new List<ItemStack>();
        private readonly List<StorageContainer> _storage = new List<StorageContainer>();
        private readonly List<CompendiumEntry> _compendium = new List<CompendiumEntry>();
        private readonly HashSet<string> _knownRecipes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<StationInfo> _stations = new List<StationInfo>();

        public string Name { get; set; } = "Tester";

        public Location Location { get; set; }

        public IReadOnlyList<ItemStack> Inventory => _inventory;

        public IReadOnlyList<CompendiumEntry> Compendium => _compendium;

        public IReadOnlyList<StationInfo> AvailableStations => _stations;

        public IReadOnlyList<StorageContainer> GetNearbyStorage(float radius) => _storage;

        public bool IsRecipeKnown(string recipeId) => recipeId != null && _knownRecipes.Contains(recipeId);

        public void AddInventory(ItemStack item)
        {
            _inventory.Add(item);
        }

        public void AddStorage(StorageContainer container)
        {
            _storage.Add(container);
        }

        public void AddCompendium(CompendiumEntry entry)
        {
            _compendium.Add(entry);
        }

        public void AddKnownRecipe(string recipeId)
        {
            if (!string.IsNullOrEmpty(recipeId))
            {
                _knownRecipes.Add(recipeId);
            }
        }

        public void AddStation(StationInfo station)
        {
            _stations.Add(station);
        }
    }
}