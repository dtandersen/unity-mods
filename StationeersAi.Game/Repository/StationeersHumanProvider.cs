using System;
using Assets.Scripts.Objects.Entities;

namespace StationeersAi.Repository;

/// <summary>
/// Reads the local player straight from the game. Null when no player
/// exists (dedicated server, main menu).
/// </summary>
public sealed class StationeersHumanProvider : IHumanProvider
{
    public Human? GetHuman()
    {
        try
        {
            return Human.LocalHuman;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
