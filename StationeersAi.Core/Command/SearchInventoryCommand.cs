using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command
{
    /// <summary>Domain operation: find carried items by name.</summary>
    public sealed class SearchInventoryCommand
    {
        private readonly InventoryIndex _inventory;

        public SearchInventoryCommand(InventoryIndex inventory)
        {
            _inventory = inventory;
        }

        public IReadOnlyList<ItemStack> Execute(string query)
        {
            return _inventory.Search(query);
        }
    }
}
