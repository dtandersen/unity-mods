using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory item repository. Used by tests and as a stand-in.</summary>
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