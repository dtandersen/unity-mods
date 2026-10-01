using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: find items whose name matches a query, as CSV.</summary>
public sealed class SearchItemsTool : ITool
{
    private readonly SearchItemsCommand _command;

    public SearchItemsTool(SearchItemsCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "search_items",
        "Search the game's items by name. Returns CSV with columns Name, Type, Id.",
        new[] { new ToolParameter("query", "Text to match against item names, e.g. 'steel'.") });

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
            .ToArray();

        return new ToolSuccess<SearchItemsResult>(
            new SearchItemsResult(items),
            FormatContent(query, items));
    }

    private static string FormatContent(string query, IReadOnlyList<Item> items)
    {
        if (items.Count == 0)
        {
            return $"No items found for '{query}'.";
        }

        var rows = items.Select(item => new[]
        {
            item.Name ?? string.Empty,
            item.Type ?? string.Empty,
            item.Id ?? string.Empty,
        });

        return Csv.Build(new[] { "Name", "Type", "Id" }, rows);
    }
}
