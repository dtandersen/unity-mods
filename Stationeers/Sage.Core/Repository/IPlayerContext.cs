using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>
/// One player's state as seen by the tools: who they are, their status, and
/// what they carry. Built per request for the local player.
/// </summary>
public interface IPlayerContext
{
    /// <summary>The player's name, or null when unavailable.</summary>
    string? Name { get; }

    /// <summary>The player's status, or null when unavailable.</summary>
    Player? Status { get; }

    /// <summary>The items the player is carrying (hands, suit, backpack...).</summary>
    IReadOnlyList<ItemStack> Inventory { get; }
}
