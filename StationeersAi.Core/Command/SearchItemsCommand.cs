using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Query;
using StationeersAi.Repository;

namespace StationeersAi.Command
{
    /// <summary>Domain operation: find items by name.</summary>
    public sealed class SearchItemsCommand
    {
        private readonly ItemIndex _items;

        public SearchItemsCommand(ItemIndex items)
        {
            _items = items;
        }

        public IReadOnlyList<Item> Execute(string query)
        {
            return _items.Search(query);
        }
    }
}
