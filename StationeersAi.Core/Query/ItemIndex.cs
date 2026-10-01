using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Query;

/// <summary>
/// Search over items. Kept out of the repository so both the in-memory and
/// game-backed repositories share identical, testable match semantics.
/// Matching is case-insensitive substring on the item name; order is not
/// significant.
/// </summary>
public sealed class ItemIndex
{
    private readonly IItemRepository _items;

    public ItemIndex(IItemRepository items)
    {
        _items = items;
    }

    public IReadOnlyList<Item> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<Item>();
        }

        return _items.GetAll()
            .Where(item => item.Name != null &&
                item.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToArray();
    }
}
