using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Repository;
using Caev.Tools;

namespace StationeersAi.Command;

/// <summary>Ask for one live thing, by label or reference id.</summary>
public sealed record DescribeThingRequest(string? Thing);

/// <summary>
/// One thing's full description, or the closest matches when no single
/// thing fits. Candidate lists stay empty on a good read and when nothing
/// matches at all.
/// </summary>
public sealed record ThingDescribeResult(
    Status Status,
    List<string> Errors,
    ThingDetails? Details,
    IReadOnlyList<StructureInfo> StructureCandidates,
    IReadOnlyList<ThingInfo> ThingCandidates) : BaseResult(Status, Errors);

/// <summary>
/// Domain operation: describe one live thing. A structure match carries
/// its full structure payload; a loose match carries identity, quantity, and
/// position. A label or id no single thing answers to raises
/// <see cref="Status.NOT_FOUND"/> with the closest matches.
/// </summary>
public sealed class DescribeThingCommand : Command<DescribeThingRequest, ThingDescribeResult>
{
    private readonly IThingInspector _inspector;
    private readonly IStructureCatalog _structures;
    private readonly IThingCatalog _things;

    public DescribeThingCommand(IThingInspector inspector, IStructureCatalog structures, IThingCatalog things)
    {
        _inspector = inspector;
        _structures = structures;
        _things = things;
    }

    public ThingDescribeResult Execute(DescribeThingRequest request)
    {
        var details = _inspector.Describe(request.Thing);
        if (details != null)
        {
            return new ThingDescribeResult(
                Status.SUCCESS,
                new List<string>(),
                details,
                Array.Empty<StructureInfo>(),
                Array.Empty<ThingInfo>());
        }

        var structureCandidates = _structures.Search(request.Thing, null, null, null, null).Take(ToolLimits.MaxResults).ToArray();
        var thingCandidates = _things.Search(request.Thing, null, null).Take(ToolLimits.MaxResults).ToArray();
        return new ThingDescribeResult(
            Status.NOT_FOUND,
            new List<string> { $"No single thing matches '{request.Thing}'." },
            null,
            structureCandidates,
            thingCandidates);
    }
}
