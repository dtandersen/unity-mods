using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>
    /// Search over items. Food search matches only items that have food stats, and
    /// like recipe search is a case-insensitive substring on the name with
    /// insignificant ordering.
    /// </summary>
    public sealed class ItemIndex
    {
        private readonly IItemRepository _items;

        public ItemIndex(IItemRepository items)
        {
            _items = items;
        }

        public IReadOnlyList<Item> SearchFood(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<Item>();
            }

            return _items.GetAll()
                .Where(item => item.Food != null &&
                               item.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
        }
    }
}