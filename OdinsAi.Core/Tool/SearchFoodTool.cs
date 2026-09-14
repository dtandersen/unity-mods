using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Command;
using OdinsAi.Entity;

namespace OdinsAi.Tool
{
    /// <summary>Tool: find food by name and return its stats as CSV.</summary>
    public sealed class SearchFoodTool : ITool
    {
        private readonly SearchFoodCommand _command;

        public SearchFoodTool(SearchFoodCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_food",
            "Search the game's food by name. Returns CSV with columns Name, Health, Stamina, Eitr, Duration (minutes), Healing (health per tick).",
            new[] { new ToolParameter("query", "Text to match against food names, e.g. 'cooked'.") });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolFailure("Missing required argument 'query'.");
            }

            var foods = _command.Execute(query)
                .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .Take(ToolLimits.MaxResults)
                .Select(ToSummary)
                .ToArray();

            return new ToolSuccess<SearchFoodResult>(
                new SearchFoodResult(foods),
                FormatContent(query, foods));
        }

        private static FoodSummary ToSummary(Item item)
        {
            return new FoodSummary(
                item.Name,
                item.Food.Health,
                item.Food.Stamina,
                item.Food.Eitr,
                item.Food.BurnTime,
                item.Food.Regen);
        }

        private static string FormatContent(string query, IReadOnlyList<FoodSummary> foods)
        {
            if (foods.Count == 0)
            {
                return $"No food found for '{query}'.";
            }

            var rows = foods.Select(food => new[]
            {
                food.Name,
                Number(food.Health),
                Number(food.Stamina),
                Number(food.Eitr),
                Number(food.BurnTime / 60f),
                Number(food.Regen),
            });

            return Csv.Build(new[] { "Name", "Health", "Stamina", "Eitr", "Duration", "Healing" }, rows);
        }

        private static string Number(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}