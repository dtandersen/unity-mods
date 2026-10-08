using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command;

/// <summary>Ask for recipes by name fragment.</summary>
public sealed record SearchRecipesRequest(string? Query);

/// <summary>The recipes that matched; empty when none did.</summary>
public sealed record RecipesResult(
    Status Status, List<string> Errors, IReadOnlyList<Recipe> Recipes) : BaseResult(Status, Errors);

/// <summary>Domain operation: search the recipe catalogue.</summary>
public sealed class SearchRecipesCommand : Command<SearchRecipesRequest, RecipesResult>
{
    private readonly RecipeIndex _recipes;

    public SearchRecipesCommand(RecipeIndex recipes)
    {
        _recipes = recipes;
    }

    public RecipesResult Execute(SearchRecipesRequest request)
    {
        return new RecipesResult(Status.SUCCESS, new List<string>(), _recipes.Search(request.Query));
    }
}
