using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>Tool: find structures whose name matches a query, as CSV.</summary>
public sealed class SearchStructuresTool : ITool
{
    private readonly SearchStructuresCommand _command;

    public SearchStructuresTool(SearchStructuresCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "search_structures",
        "Search the game's buildable structures by name. Returns CSV with columns Name, Description, Id.",
        new[] { new ToolParameter("query", "Text to match against structure names, e.g. 'solar'.") });

    public ToolResult Execute(ToolArguments arguments)
    {
        var query = arguments.Get("query");
        if (string.IsNullOrWhiteSpace(query))
        {
            return new ToolFailure("Missing required argument 'query'.");
        }

        var structures = _command.Execute(query)
            .OrderBy(structure => structure.Name, StringComparer.OrdinalIgnoreCase)
            .Take(ToolLimits.MaxResults)
            .ToArray();

        return new ToolSuccess<SearchStructuresResult>(
            new SearchStructuresResult(structures),
            FormatContent(query, structures));
    }

    private static string FormatContent(string query, IReadOnlyList<Structure> structures)
    {
        if (structures.Count == 0)
        {
            return $"No structures found for '{query}'.";
        }

        var rows = structures.Select(structure => new[]
        {
            structure.Name ?? string.Empty,
            structure.Description ?? string.Empty,
            structure.Id ?? string.Empty,
        });

        return Csv.Build(new[] { "Name", "Description", "Id" }, rows);
    }
}
