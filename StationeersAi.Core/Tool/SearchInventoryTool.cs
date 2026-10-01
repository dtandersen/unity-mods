using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;

namespace StationeersAi.Tool
{
    /// <summary>Tool: find carried items whose name matches a query, as CSV.</summary>
    public sealed class SearchInventoryTool : ITool
    {
        private readonly SearchInventoryCommand _command;

        public SearchInventoryTool(SearchInventoryCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_inventory",
            "Search what the player is carrying by name. Omit the query to list everything carried and worn. Returns CSV with columns Name, Quantity, Slot.",
            new[] { new ToolParameter("query", "Text to match against carried item names, e.g. 'welder'. Omit to list all.", Required: false) });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");
            var stacks = _command.Execute(query)
                .OrderBy(stack => stack.Name, StringComparer.OrdinalIgnoreCase)
                .Take(ToolLimits.MaxResults)
                .ToArray();

            return new ToolSuccess<SearchInventoryResult>(
                new SearchInventoryResult(stacks),
                FormatContent(query, stacks));
        }

        private static string FormatContent(string query, IReadOnlyList<ItemStack> stacks)
        {
            if (stacks.Count == 0)
            {
                return string.IsNullOrWhiteSpace(query)
                    ? "No carried items found."
                    : $"No carried items found for '{query}'.";
            }

            var rows = stacks.Select(stack => new[]
            {
                stack.Name ?? string.Empty,
                stack.Quantity ?? string.Empty,
                stack.Slot ?? string.Empty,
            });

            return Csv.Build(new[] { "Name", "Quantity", "Slot" }, rows);
        }
    }
}
