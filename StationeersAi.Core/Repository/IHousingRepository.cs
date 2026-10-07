namespace StationeersAi.Repository;

/// <summary>
/// Finds the one housing a thing id names, and builds it. Tests fill it with
/// <see cref="InMemoryHousingRepository"/>; the mod fills it with live
/// housings.
/// </summary>
public interface IHousingRepository
{
    /// <summary>Resolves one housing. Throws <see cref="ThingNotFound"/> when no housing carries the id.</summary>
    IHousing Find(long thingId);
}
