using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find carried items by name and/or type.</summary>
    public sealed class SearchInventoryCommand
    {
        private readonly InventoryIndex _index;

        public SearchInventoryCommand(InventoryIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<ItemStack> Execute(string query, string type)
        {
            return _index.Search(query, type);
        }
    }
}