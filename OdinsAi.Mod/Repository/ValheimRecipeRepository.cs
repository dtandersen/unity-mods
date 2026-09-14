using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using RecipeEntity = OdinsAi.Entity.Recipe;

namespace OdinsAi.Repository
{
    /// <summary>
    /// Reads recipes from the game's <see cref="ObjectDB"/>. Like the item
    /// repository, the snapshot is materialised on the calling (main) thread.
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
                        if (resource == null || resource.m_resItem == null)
                        {
                            continue;
                        }

                        var resourceShared = resource.m_resItem.m_itemData != null
                            ? resource.m_resItem.m_itemData.m_shared
                            : null;

                        var itemName = resourceShared != null
                            ? ValheimItemRepository.Localize(resourceShared.m_name)
                            : resource.m_resItem.name;

                        ingredients.Add(new Ingredient(itemName, resource.m_amount));
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
                    Ingredients: ingredients));
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
    }
}