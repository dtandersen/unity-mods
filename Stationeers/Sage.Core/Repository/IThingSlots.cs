using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>Contents of container things (belts, crates): what sits in each slot.</summary>
public interface IThingSlots
{
    /// <summary>Slot contents for the named thing or id, or null when no single thing matches.</summary>
    IReadOnlyList<ThingSlot>? Read(string? thing);
}
