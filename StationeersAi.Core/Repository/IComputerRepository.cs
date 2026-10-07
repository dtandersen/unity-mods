namespace StationeersAi.Repository;

/// <summary>
/// Finds the one computer a thing id names, and builds it. Tests fill it
/// with <see cref="InMemoryComputerRepository"/>; the mod fills it with live
/// boards.
/// </summary>
public interface IComputerRepository
{
    /// <summary>Resolves one computer. Never returns null.</summary>
    ComputerLookup Find(long computerId);
}
