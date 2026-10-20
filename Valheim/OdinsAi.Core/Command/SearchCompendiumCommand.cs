using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: find compendium pages by topic or text.</summary>
    public sealed class SearchCompendiumCommand
    {
        private readonly CompendiumIndex _index;

        public SearchCompendiumCommand(CompendiumIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<CompendiumEntry> Execute(string query)
        {
            return _index.Search(query);
        }
    }
}