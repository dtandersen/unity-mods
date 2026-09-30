using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>In-memory item repository. Used by tests.</summary>
    public sealed class InMemoryItemRepository : IItemRepository
    {
        private readonly List<Item> _items;

        public InMemoryItemRepository(IEnumerable<Item> items = null)
        {
            _items = items?.ToList() ?? new List<Item>();
        }

        public void Add(Item item)
        {
            _items.Add(item);
        }

        public IReadOnlyList<Item> GetAll()
        {
            return _items;
        }

        public Item GetById(string id)
        {
            return _items.FirstOrDefault(item => item.Id == id);
        }
    }
}
