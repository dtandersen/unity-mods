using System.Collections.Generic;
using Sage.Entity;
using Sage.Repository;

namespace Sage.Command;

/// <summary>Ask what one thing holds, by label or reference id.</summary>
public sealed record ReadThingSlotsRequest(string? Thing);

/// <summary>The thing's slots, or why the thing was not read.</summary>
public sealed record SlotsResult(
    Status Status, List<string> Errors, IReadOnlyList<ThingSlot>? Slots) : BaseResult(Status, Errors);

/// <summary>Domain operation: list the slots of one thing.</summary>
public sealed class ReadThingSlotsCommand : Command<ReadThingSlotsRequest, SlotsResult>
{
    private readonly IThingSlots _contents;

    public ReadThingSlotsCommand(IThingSlots contents)
    {
        _contents = contents;
    }

    public SlotsResult Execute(ReadThingSlotsRequest request)
    {
        var slots = _contents.Read(request.Thing);
        return slots == null
            ? new SlotsResult(
                Status.NOT_FOUND,
                new List<string> { $"Thing '{request.Thing}' not found." },
                null)
            : new SlotsResult(Status.SUCCESS, new List<string>(), slots);
    }
}
