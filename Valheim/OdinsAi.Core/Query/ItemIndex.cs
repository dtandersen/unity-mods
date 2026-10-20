using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>
    /// Search over the game's items. Matching is a case-insensitive substring on
    /// the name with insignificant ordering.
    /// </summary>
    public sealed class ItemIndex
    {
        private readonly IItemRepository _items;

        public ItemIndex(IItemRepository items)
        {
            _items = items;
        }

        /// <summary>All items whose name matches <paramref name="query"/>.</summary>
        public IReadOnlyList<Item> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<Item>();
            }

            return _items.GetAll()
                .Where(item => item.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
        }

        /// <summary>Only food items whose name matches <paramref name="query"/>.</summary>
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

        /// <summary>The distinct item types present, sorted.</summary>
        public IReadOnlyList<string> GetTypes()
        {
            return _items.GetAll()
                .Select(item => item.Type)
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(type => type, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}