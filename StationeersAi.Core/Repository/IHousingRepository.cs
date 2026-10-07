namespace StationeersAi.Repository;

/// <summary>
/// Finds the one housing a label or thing id names, and builds it. Tests
/// fill it with <see cref="InMemoryHousingRepository"/>; the mod fills it
/// with live housings.
/// </summary>
public interface IHousingRepository
{
    /// <summary>Resolves one housing. Never returns null.</summary>
    HousingLookup Find(string? device);
}
