using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Objects.Items;
using StationeersAi.Entity;
using IngredientEntity = StationeersAi.Entity.Ingredient;
using RecipeEntity = StationeersAi.Entity.Recipe;

namespace StationeersAi.Repository;

/// <summary>
/// Reads manufacturing recipes from the game's recipe tables (the static
/// <c>List&lt;RecipeReference&gt;</c> fields on <c>ElectronicReader</c>: the
/// catalogue plus the per-machine lists). Reflecting over the fields instead
/// of naming them keeps this working when the game adds machines.
/// Reagent quantities become ingredients; the creator thing names the station.
/// Snapshotted on the calling (main) thread.
/// </summary>
public sealed class StationeersRecipeRepository : IRecipeRepository
{
    private readonly List<RecipeEntity> _recipes = new List<RecipeEntity>();

    public StationeersRecipeRepository()
        : this(RecipeTables())
    {
    }

    internal StationeersRecipeRepository(IEnumerable<RecipeReference> references)
    {
        if (references == null)
        {
            return;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var reference in references)
        {
            if (reference?.DynamicThing == null)
            {
                continue;
            }

            var output = reference.DynamicThing;
            var station = StationName(reference);
            var key = $"{output.PrefabName}@{station}";
            if (!seen.Add(key))
            {
                continue;
            }

            _recipes.Add(new RecipeEntity(
                Id: output.PrefabName,
                ResultItemId: output.PrefabName,
                Name: DisplayName(output),
                Amount: 1,
                Station: station,
                Ingredients: Ingredients(reference.Recipe)));
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

    private static IEnumerable<RecipeReference> RecipeTables()
    {
        var tables = new List<RecipeReference>();
        foreach (var field in typeof(ElectronicReader).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(List<RecipeReference>))
            {
                continue;
            }

            var list = field.GetValue(null) as List<RecipeReference>;
            if (list != null)
            {
                tables.AddRange(list);
            }
        }

        return tables;
    }

    private static string StationName(RecipeReference reference)
    {
        if (reference.Creator == null)
        {
            return null;
        }

        return DisplayName(reference.Creator);
    }

    private static string DisplayName(Assets.Scripts.Objects.Thing thing)
    {
        try
        {
            var name = thing.DisplayName;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }
        catch (Exception)
        {
        }

        return string.IsNullOrWhiteSpace(thing.PrefabName) ? "Unknown" : thing.PrefabName;
    }

    /// <summary>
    /// Non-zero reagent fields of the recipe struct, e.g. Iron x6. Read with
    /// reflection so new reagents do not need code changes.
    /// </summary>
    internal static IReadOnlyList<IngredientEntity> Ingredients(Reagents.Recipe recipe)
    {
        var ingredients = new List<IngredientEntity>();
        foreach (var field in typeof(Reagents.Recipe).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType != typeof(double))
            {
                continue;
            }

            var amount = (double)field.GetValue(recipe);
            if (amount > 0)
            {
                ingredients.Add(new IngredientEntity(field.Name, amount));
            }
        }

        return ingredients;
    }
}
