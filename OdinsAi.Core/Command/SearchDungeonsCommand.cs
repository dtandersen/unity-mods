using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find dungeon entrances near the player.</summary>
    public sealed class SearchDungeonsCommand
    {
        /// <summary>Search radius, in metres.</summary>
        public const float Radius = 500f;

        private readonly DungeonIndex _index;

        public SearchDungeonsCommand(DungeonIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<Dungeon> Execute(string query)
        {
            return _index.Search(Radius, query);
        }
    }
}