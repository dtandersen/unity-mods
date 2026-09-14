using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>Case-insensitive substring search over the player's carried items.</summary>
    public sealed class InventoryIndex
    {
        private readonly IInventoryRepository _inventory;

        public InventoryIndex(IInventoryRepository inventory)
        {
            _inventory = inventory;
        }

        /// <summary>
        /// Matches on name (<paramref name="query"/>) and/or coarse type
        /// (<paramref name="type"/>, e.g. <c>food</c>). Either may be null.
        /// </summary>
        public IReadOnlyList<ItemStack> Search(string query, string type)
        {
            var hasQuery = !string.IsNullOrWhiteSpace(query);
            var hasType = !string.IsNullOrWhiteSpace(type);

            if (!hasQuery && !hasType)
            {
                return Array.Empty<ItemStack>();
            }

            return _inventory.GetCarriedItems()
                .Where(item =>
                    (!hasQuery || item.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (!hasType || string.Equals(item.Type, type, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }
    }
}