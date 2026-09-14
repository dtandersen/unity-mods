using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Command;

namespace OdinsAi.Tool
{
    /// <summary>
    /// Tool: search the player's carried items by name and/or type. Returns totals
    /// per item as CSV.
    /// </summary>
    public sealed class SearchInventoryTool : ITool
    {
        private readonly SearchInventoryCommand _command;

        public SearchInventoryTool(SearchInventoryCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_inventory",
            "Search the player's carried items by name and/or type (e.g. type 'food'). Returns CSV with columns Name, Amount.",
            new[]
            {
                new ToolParameter("query", "Text to match against item names, e.g. 'turnip'. Optional if a type is given.", Required: false),
                new ToolParameter("type", "Optional item type filter, e.g. 'food'.", Required: false),
            });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");
            var type = arguments.Get("type");

            if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(type))
            {
                return new ToolFailure("Provide a 'query' and/or a 'type'.");
            }

            var items = _command.Execute(query, type)
                .GroupBy(item => item.Name)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Take(ToolLimits.MaxResults)
                .Select(group => new ItemStackSummary(group.Key, group.Sum(item => item.Amount)))
                .ToArray();

            return new ToolSuccess<InventorySearchResult>(
                new InventorySearchResult(items),
                FormatContent(query, type, items));
        }

        private static string FormatContent(string query, string type, IReadOnlyList<ItemStackSummary> items)
        {
            if (items.Count == 0)
            {
                var what = !string.IsNullOrWhiteSpace(type) ? $"type '{type}'" : $"'{query}'";
                return $"No items matching {what} in your inventory.";
            }

            return Csv.Build(
                new[] { "Name", "Amount" },
                items.Select(item => new[] { item.Name, item.Amount.ToString(CultureInfo.InvariantCulture) }));
        }
    }
}