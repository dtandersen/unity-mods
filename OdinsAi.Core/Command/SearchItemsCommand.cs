using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: look up items in the game's item list by name.</summary>
    public sealed class SearchItemsCommand
    {
        private readonly ItemIndex _index;

        public SearchItemsCommand(ItemIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<Item> Execute(string query)
        {
            return _index.Search(query);
        }
    }
}