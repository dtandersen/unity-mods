using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find creatures near the player.</summary>
    public sealed class SearchCreaturesCommand
    {
        /// <summary>Search radius, in metres.</summary>
        public const float Radius = 100f;

        private readonly CreatureIndex _index;

        public SearchCreaturesCommand(CreatureIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<Creature> Execute(string query)
        {
            return _index.Search(Radius, query);
        }
    }
}