using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command;

/// <summary>Ask for structures by name fragment.</summary>
public sealed record SearchStructuresRequest(string? Query);

/// <summary>The structures that matched; empty when none did.</summary>
public sealed record StructuresResult(
    Status Status, List<string> Errors, IReadOnlyList<Structure> Structures) : BaseResult(Status, Errors);

/// <summary>Domain operation: search the structure catalogue.</summary>
public sealed class SearchStructuresCommand : Command<SearchStructuresRequest, StructuresResult>
{
    private readonly StructureIndex _structures;

    public SearchStructuresCommand(StructureIndex structures)
    {
        _structures = structures;
    }

    public StructuresResult Execute(SearchStructuresRequest request)
    {
        return new StructuresResult(Status.SUCCESS, new List<string>(), _structures.Search(request.Query));
    }
}
