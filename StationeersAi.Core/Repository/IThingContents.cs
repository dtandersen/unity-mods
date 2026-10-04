using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>Contents of container things (belts, crates): what sits in each slot.</summary>
public interface IThingContents
{
    /// <summary>Slot contents for the named thing or id, or null when no single thing matches.</summary>
    IReadOnlyList<ThingContent> Read(string thing);
}
