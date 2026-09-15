using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Query;
using OdinsAi.Repository;

namespace OdinsAi.Command
{
    /// <summary>
    /// Domain operation: find recipes by name and report the unlock state of each
    /// quality upgrade. An upgrade is unlocked when the recipe is known and a
    /// station of the required type and level is available to the player.
    /// </summary>
    public sealed class SearchUpgradesCommand
    {
        private readonly RecipeIndex _recipes;
        private readonly IPlayerContext _player;

        public SearchUpgradesCommand(RecipeIndex recipes, IPlayerContext player)
        {
            _recipes = recipes;
            _player = player;
        }

        public IReadOnlyList<UnlockedUpgrade> Execute(string query)
        {
            var upgrades = new List<UnlockedUpgrade>();

            foreach (var recipe in _recipes.Search(query))
            {
                if (recipe.Upgrades == null || recipe.Upgrades.Count == 0)
                {
                    continue;
                }

                var known = _player.IsRecipeKnown(recipe.Id);
                foreach (var upgrade in recipe.Upgrades)
                {
                    upgrades.Add(new UnlockedUpgrade(recipe, upgrade, known && IsStationAvailable(upgrade)));
                }
            }

            return upgrades;
        }

        private bool IsStationAvailable(UpgradeLevel upgrade)
        {
            if (upgrade.Station == null)
            {
                return true;
            }

            return _player.AvailableStations.Any(station =>
                string.Equals(station.Name, upgrade.Station, StringComparison.OrdinalIgnoreCase) &&
                station.Level >= upgrade.StationLevel);
        }
    }
}