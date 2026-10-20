using Sage.Entity;

namespace Sage.Repository;

/// <summary>Full details of one live thing, structure or loose.</summary>
public interface IThingInspector
{
    /// <summary>Details for the named thing, or null when no single thing matches.</summary>
    ThingDetails? Describe(string? thing);
}
