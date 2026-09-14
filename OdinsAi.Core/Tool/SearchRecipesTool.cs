using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Command;
using OdinsAi.Entity;

namespace OdinsAi.Tool
{
    /// <summary>Tool: find recipes whose name matches a query.</summary>
    public sealed class SearchRecipesTool : ITool
    {
        private readonly SearchRecipesCommand _command;

        public SearchRecipesTool(SearchRecipesCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_recipes",
            "Search the game's recipes by name. Use for questions like 'how do I make stew?'.",
            new[] { new ToolParameter("query", "Text to match against recipe names, e.g. 'stew'.") });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolFailure("Missing required argument 'query'.");
            }

            var summaries = _command.Execute(query)
                .OrderBy(recipe => recipe.Name, StringComparer.OrdinalIgnoreCase)
                .Take(ToolLimits.MaxResults)
                .Select(ToSummary)
                .ToArray();

            return new ToolSuccess<SearchRecipesResult>(
                new SearchRecipesResult(summaries),
                FormatContent(query, summaries));
        }

        private static RecipeSummary ToSummary(Recipe recipe)
        {
            return new RecipeSummary(
                recipe.Name,
                recipe.CraftingStation,
                recipe.Ingredients.Select(i => new IngredientResult(i.ItemId, i.Amount)).ToArray());
        }

        private static string FormatContent(string query, IReadOnlyList<RecipeSummary> summaries)
        {
            if (summaries.Count == 0)
            {
                return $"No recipes found for '{query}'.";
            }

            return string.Join("\n", summaries.Select(summary =>
                $"{summary.Name}: " +
                string.Join(", ", summary.Ingredients.Select(i => $"{i.ItemId} x{i.Amount}"))));
        }
    }
}