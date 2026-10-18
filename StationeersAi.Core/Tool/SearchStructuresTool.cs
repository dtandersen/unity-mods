using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Command;
using StationeersAi.Entity;
using Caev.Tools;

namespace StationeersAi.Tool;

/// <summary>Tool: find structures whose name matches a query, with build steps.</summary>
public sealed class SearchStructuresTool : ITool
{
    private readonly SearchStructuresCommand _command;

    public SearchStructuresTool(SearchStructuresCommand command)
    {
        _command = command;
    }

    public ToolDefinition Definition { get; } = new ToolDefinition(
        "search_structures",
        "Search the game's buildable structures by name. Returns each match with its Name, Description, and build Steps, one line per stage (e.g. '2. Wrench - Iron Sheets x2').",
        new[] { new ToolParameter("query", "Text to match against structure names, e.g. 'solar'.") });

    public ToolResult Execute(ToolArguments arguments)
    {
        var query = arguments.Get("query");
        if (string.IsNullOrWhiteSpace(query))
        {
            return new ToolFailure("Missing required argument 'query'.");
        }

        var structures = _command.Execute(new SearchStructuresRequest(query)).Structures
            .OrderBy(structure => structure.Name, StringComparer.OrdinalIgnoreCase)
            .Take(ToolLimits.MaxResults)
            .ToArray();

        return new ToolSuccess<SearchStructuresResult>(
            new SearchStructuresResult(structures),
            FormatContent(query, structures));
    }

    private static string FormatContent(string? query, IReadOnlyList<Structure> structures)
    {
        if (structures.Count == 0)
        {
            return $"No structures found for '{query}'.";
        }

        return string.Join("\n\n", structures.Select(FormatStructure));
    }

    private static string FormatStructure(Structure structure)
    {
        var lines = new List<string>
        {
            $"Name: {structure.Name ?? string.Empty}",
            $"Description: {structure.Description ?? string.Empty}",
            "Steps:",
        };

        foreach (var step in structure.BuildStates ?? Array.Empty<BuildStep>())
        {
            lines.Add(BuildStepText.Format(step));
        }

        return string.Join("\n", lines);
    }
}
