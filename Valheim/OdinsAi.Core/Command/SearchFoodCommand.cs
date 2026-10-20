using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find food items whose name matches a query.</summary>
    public sealed class SearchFoodCommand
    {
        private readonly ItemIndex _index;

        public SearchFoodCommand(ItemIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<Item> Execute(string query)
        {
            return _index.SearchFood(query);
        }
    }
}