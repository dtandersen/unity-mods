using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>In-memory thing contents. Used by tests.</summary>
public sealed class InMemoryThingSlots : IThingSlots
{
    public List<ThingSlot> Contents { get; } = new List<ThingSlot>();

    public string? LastThing { get; private set; }

    public bool HasMatch { get; set; } = true;

    public IReadOnlyList<ThingSlot>? Read(string? thing)
    {
        LastThing = thing;
        return HasMatch ? Contents : null;
    }
}
