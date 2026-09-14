using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Command;

namespace OdinsAi.Tool
{
    /// <summary>
    /// Tool: search nearby containers (within 100 metres) for items. Returns totals
    /// per item as CSV.
    /// </summary>
    public sealed class SearchStorageTool : ITool
    {
        private readonly SearchStorageCommand _command;

        public SearchStorageTool(SearchStorageCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_storage",
            "Search containers near the player (within 100 metres). Returns CSV with columns Name, Amount.",
            new[] { new ToolParameter("query", "Text to match against item names, e.g. 'turnip'.") });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolFailure("Missing required argument 'query'.");
            }

            var items = _command.Execute(query)
                .SelectMany(container => container.Items)
                .GroupBy(item => item.Name)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Take(ToolLimits.MaxResults)
                .Select(group => new ItemStackSummary(group.Key, group.Sum(item => item.Amount)))
                .ToArray();

            return new ToolSuccess<StorageSearchResult>(
                new StorageSearchResult(items),
                FormatContent(query, items));
        }

        private static string FormatContent(string query, IReadOnlyList<ItemStackSummary> items)
        {
            if (items.Count == 0)
            {
                return $"No items matching '{query}' in nearby containers.";
            }

            return Csv.Build(
                new[] { "Name", "Amount" },
                items.Select(item => new[] { item.Name, item.Amount.ToString(CultureInfo.InvariantCulture) }));
        }
    }
}