using System;
using System.Collections.Generic;
using System.Linq;
using Sage.Entity;
using Sage.Repository;
using Caev.Tools;

namespace Sage.Command;

/// <summary>Ask for one structure, by label or reference id.</summary>
public sealed record DescribeStructureRequest(string? Structure);

/// <summary>
/// One structure's full description, or the brief matches when no single entry
/// fits. <see cref="Candidates"/> stays empty on a good read and when
/// nothing matches at all.
/// </summary>
public sealed record StructureDescribeResult(
    Status Status,
    List<string> Errors,
    StructureDetails? Details,
    IReadOnlyList<StructureInfo> Candidates) : BaseResult(Status, Errors);

/// <summary>
/// Domain operation: describe one live structure. A label or id no single
/// structure answers to raises <see cref="Status.NOT_FOUND"/> with the brief
/// matches that came closest.
/// </summary>
public sealed class DescribeStructureCommand : Command<DescribeStructureRequest, StructureDescribeResult>
{
    private readonly IStructureInspector _inspector;
    private readonly IStructureCatalog _structures;

    public DescribeStructureCommand(IStructureInspector inspector, IStructureCatalog structures)
    {
        _inspector = inspector;
        _structures = structures;
    }

    public StructureDescribeResult Execute(DescribeStructureRequest request)
    {
        var details = _inspector.Describe(request.Structure);
        if (details != null)
        {
            return new StructureDescribeResult(Status.SUCCESS, new List<string>(), details, Array.Empty<StructureInfo>());
        }

        var candidates = _structures.Search(request.Structure, null, null, null, null).Take(ToolLimits.MaxResults).ToArray();
        return new StructureDescribeResult(
            Status.NOT_FOUND,
            new List<string> { $"No single structure matches '{request.Structure}'." },
            null,
            candidates);
    }
}
