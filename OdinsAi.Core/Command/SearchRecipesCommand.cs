using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>
    /// Domain operation: find recipes whose name matches a query. Knows nothing
    /// about the model or tool schemas; that is the tool's job.
    /// </summary>
    public sealed class SearchRecipesCommand
    {
        private readonly RecipeIndex _index;

        public SearchRecipesCommand(RecipeIndex index)
        {
            _index = index;
        }

        public IReadOnlyList<Recipe> Execute(string query)
        {
            return _index.Search(query);
        }
    }
}