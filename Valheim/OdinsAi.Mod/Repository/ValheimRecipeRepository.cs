using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using RecipeEntity = OdinsAi.Entity.Recipe;

namespace OdinsAi.Repository
{
    /// <summary>
    /// Reads recipes from the game's <see cref="ObjectDB"/>. This is world data:
    /// whether the player has unlocked a recipe, or can reach a station, lives in
    /// <see cref="ValheimPlayerContext"/>. The snapshot is materialised on the
    /// calling (main) thread.
    /// </summary>
    public sealed class ValheimRecipeRepository : IRecipeRepository
    {
        private readonly List<RecipeEntity> _recipes = new List<RecipeEntity>();

        public ValheimRecipeRepository(ObjectDB database)
        {
            if (database == null || database.m_recipes == null)
            {
                return;
            }

            foreach (var recipe in database.m_recipes)
            {
                if (recipe == null || recipe.m_item == null)
                {
                    continue;
                }

                var shared = recipe.m_item.m_itemData != null ? recipe.m_item.m_itemData.m_shared : null;
                var name = shared != null
                    ? ValheimItemRepository.Localize(shared.m_name)
                    : recipe.m_item.name;

                var ingredients = new List<Ingredient>();
                if (recipe.m_resources != null)
                {
                    foreach (var resource in recipe.m_resources)
                    {
                        // Skip upgrader-only resources (e.g. the 1.0 "idols" used at a hidden
                        // forge). They are not part of the base crafting recipe.
                        if (resource == null || resource.m_resItem == null || resource.m_upgraderResource)
                        {
                            continue;
                        }

                        ingredients.Add(new Ingredient(ResourceName(resource), resource.m_amount));
                    }
                }

                _recipes.Add(new RecipeEntity(
                    Id: recipe.m_item.name,
                    ResultItemId: recipe.m_item.name,
                    Name: name,
                    Amount: recipe.m_amount,
                    CraftingStation: recipe.m_craftingStation != null
                        ? ValheimItemRepository.Localize(recipe.m_craftingStation.m_name)
                        : null,
                    Ingredients: ingredients,
                    Upgrades: Upgrades(recipe, shared)));
            }
        }

        public IReadOnlyList<RecipeEntity> GetAll()
        {
            return _recipes;
        }

        public RecipeEntity GetById(string id)
        {
            return _recipes.FirstOrDefault(recipe => recipe.Id == id);
        }

        /// <summary>
        /// The cost of each quality upgrade, mirroring InventoryGui: for every target
        /// quality, keep the requirements that apply at the recipe's station (by the
        /// upgrader flag) and take <see cref="Piece.Requirement.GetAmount"/>.
        /// </summary>
        private static IReadOnlyList<UpgradeLevel> Upgrades(Recipe recipe, ItemDrop.ItemData.SharedData shared)
        {
            var maxQuality = shared != null ? shared.m_maxQuality : 1;
            if (maxQuality <= 1 || recipe.m_resources == null)
            {
                return System.Array.Empty<UpgradeLevel>();
            }

            var upgrades = new List<UpgradeLevel>();
            for (var quality = 2; quality <= maxQuality; quality++)
            {
                var station = recipe.GetRequiredStation(quality);

                var ingredients = new List<Ingredient>();
                foreach (var resource in recipe.m_resources)
                {
                    if (resource == null || resource.m_resItem == null)
                    {
                        continue;
                    }

                    var applies = station != null
                        ? station.m_upgrader == resource.m_upgraderResource
                        : !resource.m_upgraderResource;
                    if (!applies)
                    {
                        continue;
                    }

                    var amount = resource.GetAmount(quality);
                    if (amount > 0)
                    {
                        ingredients.Add(new Ingredient(ResourceName(resource), amount));
                    }
                }

                upgrades.Add(new UpgradeLevel(
                    quality,
                    station != null ? ValheimItemRepository.Localize(station.m_name) : null,
                    recipe.GetRequiredStationLevel(quality),
                    ingredients));
            }

            return upgrades;
        }

        private static string ResourceName(Piece.Requirement resource)
        {
            var shared = resource.m_resItem.m_itemData != null
                ? resource.m_resItem.m_itemData.m_shared
                : null;

            return shared != null
                ? ValheimItemRepository.Localize(shared.m_name)
                : resource.m_resItem.name;
        }
    }
}