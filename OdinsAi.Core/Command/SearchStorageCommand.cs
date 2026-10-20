using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find items matching a query in nearby containers.</summary>
    public sealed class SearchStorageCommand
    {
        /// <summary>Search radius, in metres.</summary>
        public const float Radius = 100f;

        private readonly StorageIndex _index;

        public SearchStorageCommand(StorageIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<StorageContainer> Execute(string query)
        {
            return _index.Search(Radius, query);
        }
    }
}