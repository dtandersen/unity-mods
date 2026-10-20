using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Command;
using OdinsAi.Entity;

namespace OdinsAi.Tool
{
    /// <summary>
    /// Tool: list resource nodes (ore deposits, pickables, trees) near the player,
    /// nearest first.
    /// </summary>
    public sealed class SearchResourcesTool : ITool
    {
        private readonly SearchResourcesCommand _command;

        public SearchResourcesTool(SearchResourcesCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_resources",
            "List resource nodes near the player (within 100 metres), nearest first: ore deposits, pickables (berries, mushrooms) and trees. Returns CSV with columns Name, Type, Distance (m), Direction. Use for questions like 'is there any tin nearby?'.",
            new[] { new ToolParameter("query", "Optional text to match against resource names or types (ore, pickable, tree), e.g. 'tin'.", Required: false) });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");

            var resources = _command.Execute(query)
                .Take(ToolLimits.MaxResults)
                .Select(resource => new ResourceSummary(resource.Name, resource.Type, resource.Distance, resource.Direction))
                .ToArray();

            return new ToolSuccess<SearchResourcesResult>(
                new SearchResourcesResult(resources),
                FormatContent(query, resources));
        }

        private static string FormatContent(string query, IReadOnlyList<ResourceSummary> resources)
        {
            if (resources.Count == 0)
            {
                return string.IsNullOrWhiteSpace(query)
                    ? "No resources found nearby."
                    : $"No resources matching '{query}' nearby.";
            }

            return Csv.Build(
                new[] { "Name", "Type", "Distance (m)", "Direction" },
                resources.Select(resource => new[]
                {
                    resource.Name,
                    resource.Type,
                    Number(resource.Distance),
                    resource.Direction,
                }));
        }

        private static string Number(float value)
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }
    }
}