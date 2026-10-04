using Assets.Scripts.Objects.Entities;

namespace StationeersAi.Repository;

/// <summary>
/// Supplies the local player on every call, so readers hold no snapshot
/// and never go stale. Null when no player exists (dedicated server,
/// main menu).
/// </summary>
public interface IHumanProvider
{
    Human? GetHuman();
}
