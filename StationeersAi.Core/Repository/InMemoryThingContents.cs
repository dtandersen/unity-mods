using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory thing contents. Used by tests.</summary>
public sealed class InMemoryThingContents : IThingContents
{
    public List<ThingContent> Contents { get; } = new List<ThingContent>();

    public string? LastThing { get; private set; }

    public bool HasMatch { get; set; } = true;

    public IReadOnlyList<ThingContent>? Read(string? thing)
    {
        LastThing = thing;
        return HasMatch ? Contents : null;
    }
}
