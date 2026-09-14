using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find resource nodes near the player.</summary>
    public sealed class SearchResourcesCommand
    {
        /// <summary>Search radius, in metres.</summary>
        public const float Radius = 100f;

        private readonly ResourceIndex _index;

        public SearchResourcesCommand(ResourceIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<ResourceNode> Execute(string query)
        {
            return _index.Search(Radius, query);
        }
    }
}