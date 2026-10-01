using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: find Stationpedia pages whose title matches a query, as CSV.</summary>
public sealed class SearchStationpediaTool : ITool
{
    private readonly SearchStationpediaCommand _command;

    public SearchStationpediaTool(SearchStationpediaCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "search_stationpedia",
        "Search the in-game encyclopedia (Stationpedia) by title. Returns CSV with columns Title, Key, Description. Use read_stationpedia with a Key to read the full page.",
        new[] { new ToolParameter("query", "Text to match against page titles, e.g. 'steel'.") });

    public ToolResult Execute(ToolArguments arguments)
    {
        var query = arguments.Get("query");
        if (string.IsNullOrWhiteSpace(query))
        {
            return new ToolFailure("Missing required argument 'query'.");
        }

        var entries = _command.Execute(query)
            .OrderBy(entry => entry.Title, StringComparer.OrdinalIgnoreCase)
            .Take(ToolLimits.MaxResults)
            .ToArray();

        return new ToolSuccess<SearchStationpediaResult>(
            new SearchStationpediaResult(entries),
            FormatContent(query, entries));
    }

    private static string FormatContent(string query, IReadOnlyList<StationpediaEntry> entries)
    {
        if (entries.Count == 0)
        {
            return $"No Stationpedia pages found for '{query}'.";
        }

        var rows = entries.Select(entry => new[]
        {
            entry.Title ?? string.Empty,
            entry.Key ?? string.Empty,
            entry.Description ?? string.Empty,
        });

        return Csv.Build(new[] { "Title", "Key", "Description" }, rows);
    }
}
