using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>Tool: find recipes whose result matches a query, as CSV.</summary>
public sealed class SearchRecipesTool : ITool
{
    private readonly SearchRecipesCommand _command;

    public SearchRecipesTool(SearchRecipesCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "search_recipes",
        "Search the game's manufacturing recipes by result name. Returns CSV with columns Name, Station, Ingredients.",
        new[] { new ToolParameter("query", "Text to match against recipe names, e.g. 'cable'.") });

    public ToolResult Execute(ToolArguments arguments)
    {
        var query = arguments.Get("query");
        if (string.IsNullOrWhiteSpace(query))
        {
            return new ToolFailure("Missing required argument 'query'.");
        }

        var recipes = _command.Execute(query)
            .OrderBy(recipe => recipe.Name, StringComparer.OrdinalIgnoreCase)
            .Take(ToolLimits.MaxResults)
            .ToArray();

        return new ToolSuccess<SearchRecipesResult>(
            new SearchRecipesResult(recipes),
            FormatContent(query, recipes));
    }

    private static string FormatContent(string query, IReadOnlyList<Recipe> recipes)
    {
        if (recipes.Count == 0)
        {
            return $"No recipes found for '{query}'.";
        }

        var rows = recipes.Select(recipe => new[]
        {
            recipe.Name ?? string.Empty,
            recipe.Station ?? string.Empty,
            string.Join("; ", recipe.Ingredients.Select(i => $"{i.ItemId} x{FormatAmount(i.Amount)}")),
        });

        return Csv.Build(new[] { "Name", "Station", "Ingredients" }, rows);
    }

    internal static string FormatAmount(double amount)
    {
        // Reagent quantities are whole numbers in practice; trim the noise.
        return amount == Math.Truncate(amount)
            ? ((long)amount).ToString()
            : amount.ToString("0.##");
    }
}
