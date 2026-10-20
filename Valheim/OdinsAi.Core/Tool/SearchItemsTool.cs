using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Command;
using OdinsAi.Entity;

namespace OdinsAi.Tool
{
    /// <summary>
    /// Tool: look up items in the game's item list by name. Useful for recognising
    /// ingredients the model doesn't otherwise know about.
    /// </summary>
    public sealed class SearchItemsTool : ITool
    {
        private readonly SearchItemsCommand _command;

        public SearchItemsTool(SearchItemsCommand command)
        {
            _command = command;
            Definition = new ToolDefinition(
                "search_items",
                $"Look up items in the game's item list by name. Returns CSV with columns Name, Type. Known types: {string.Join(", ", command.GetTypes())}.",
                new[] { new ToolParameter("query", "Text to match against item names, e.g. 'battle idol'.") });
        }

        public ToolDefinition Definition { get; }

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolFailure("Missing required argument 'query'.");
            }

            var items = _command.Execute(query)
                .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .Take(ToolLimits.MaxResults)
                .Select(item => new ItemSummary(item.Name, item.Type))
                .ToArray();

            return new ToolSuccess<SearchItemsResult>(
                new SearchItemsResult(items),
                FormatContent(query, items));
        }

        private static string FormatContent(string query, IReadOnlyList<ItemSummary> items)
        {
            if (items.Count == 0)
            {
                return $"No items found matching '{query}'.";
            }

            return Csv.Build(
                new[] { "Name", "Type" },
                items.Select(item => new[] { item.Name, item.Type }));
        }
    }
}