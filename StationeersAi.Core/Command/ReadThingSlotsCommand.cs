using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: list what a container thing holds.</summary>
public sealed class ReadThingSlotsCommand
{
    private readonly IThingSlots _contents;

    public ReadThingSlotsCommand(IThingSlots contents)
    {
        _contents = contents;
    }

    public IReadOnlyList<ThingSlot>? Execute(string? thing)
    {
        return _contents.Read(thing);
    }
}
