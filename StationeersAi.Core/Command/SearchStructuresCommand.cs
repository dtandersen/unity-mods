using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Ask for structures by name fragment, power and data networks, logic, and room.</summary>
public sealed record SearchStructuresRequest(string? Query, long? PowerNetworkId, long? DataNetworkId, bool? Logic, long? Room);

/// <summary>The structures that matched; empty when none did.</summary>
public sealed record StructuresResult(
    Status Status, List<string> Errors, IReadOnlyList<StructureInfo> Structures) : BaseResult(Status, Errors);

/// <summary>Domain operation: search the structure catalogue.</summary>
public sealed class SearchStructuresCommand : Command<SearchStructuresRequest, StructuresResult>
{
    private readonly IStructureCatalog _structures;

    public SearchStructuresCommand(IStructureCatalog structures)
    {
        _structures = structures;
    }

    public StructuresResult Execute(SearchStructuresRequest request)
    {
        return new StructuresResult(
            Status.SUCCESS,
            new List<string>(),
            _structures.Search(request.Query, request.PowerNetworkId, request.DataNetworkId, request.Logic, request.Room));
    }
}
