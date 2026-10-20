using Sage.Entity;

namespace Sage.Repository;

/// <summary>In-memory thing inspector. Used by tests.</summary>
public sealed class InMemoryThingInspector : IThingInspector
{
    public ThingDetails? Details { get; set; }

    public string? LastThing { get; private set; }

    public ThingDetails? Describe(string? thing)
    {
        LastThing = thing;
        return Details;
    }
}
