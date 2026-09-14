using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory inventory. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryInventoryRepository : IInventoryRepository
    {
        private readonly List<ItemStack> _items = new List<ItemStack>();

        public void Add(ItemStack item)
        {
            _items.Add(item);
        }

        public IReadOnlyList<ItemStack> GetCarriedItems()
        {
            return _items;
        }
    }
}