using System;
using System.Collections.Generic;
using System.Linq;
using Sage.Entity;
using Sage.Repository;

namespace Sage.Query;

/// <summary>
/// Search over the player's carried items. Matching is case-insensitive
/// substring on the stack name; order is not significant.
/// </summary>
public sealed class InventoryIndex
{
    private readonly IPlayerContext _player;

    public InventoryIndex(IPlayerContext player)
    {
        _player = player;
    }

    public IReadOnlyList<ItemStack> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _player.Inventory.ToArray();
        }

        return _player.Inventory
            .Where(stack => stack.Name != null &&
                stack.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToArray();
    }
}
