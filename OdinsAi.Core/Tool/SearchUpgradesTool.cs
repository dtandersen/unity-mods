using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Command;

namespace OdinsAi.Tool
{
    /// <summary>
    /// Tool: find what it costs to upgrade an item to each quality. Complements
    /// <c>search_recipes</c>, which only gives the base crafting cost.
    /// </summary>
    public sealed class SearchUpgradesTool : ITool
    {
        private readonly SearchUpgradesCommand _command;

        public SearchUpgradesTool(SearchUpgradesCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_upgrades",
            "Find what it costs to upgrade an item to each quality at a crafting station. Returns CSV with columns Item, Quality, Station, Station Level, Ingredients, Unlocked. Unlocked means the recipe is known and a station of the required level is nearby. Use search_recipes for the base crafting cost.",
            new[] { new ToolParameter("query", "Text to match against item names, e.g. 'bronze axe'.") });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolFailure("Missing required argument 'query'.");
            }

            var upgrades = _command.Execute(query)
                .OrderBy(upgrade => upgrade.Recipe.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(upgrade => upgrade.Upgrade.Quality)
                .Take(ToolLimits.MaxResults)
                .Select(upgrade => new UpgradeSummary(
                    upgrade.Recipe.Name,
                    upgrade.Upgrade.Quality,
                    upgrade.Upgrade.Station,
                    upgrade.Upgrade.StationLevel,
                    upgrade.Upgrade.Ingredients.Select(i => new IngredientResult(i.ItemId, i.Amount)).ToArray(),
                    upgrade.Unlocked))
                .ToArray();

            return new ToolSuccess<SearchUpgradesResult>(
                new SearchUpgradesResult(upgrades),
                FormatContent(query, upgrades));
        }

        private static string FormatContent(string query, IReadOnlyList<UpgradeSummary> upgrades)
        {
            if (upgrades.Count == 0)
            {
                return $"No upgrade costs found for '{query}'.";
            }

            var rows = upgrades.Select(upgrade => new[]
            {
                upgrade.Item,
                upgrade.Quality.ToString(),
                upgrade.Station ?? string.Empty,
                upgrade.StationLevel.ToString(),
                string.Join("; ", upgrade.Ingredients.Select(i => $"{i.ItemId} x{i.Amount}")),
                upgrade.Unlocked ? "Yes" : "No",
            });

            return Csv.Build(new[] { "Item", "Quality", "Station", "Station Level", "Ingredients", "Unlocked" }, rows);
        }
    }
}