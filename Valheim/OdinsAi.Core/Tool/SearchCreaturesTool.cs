using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Command;
using OdinsAi.Entity;

namespace OdinsAi.Tool
{
    /// <summary>
    /// Tool: list creatures (monsters and animals) near the player, nearest first.
    /// </summary>
    public sealed class SearchCreaturesTool : ITool
    {
        private readonly SearchCreaturesCommand _command;

        public SearchCreaturesTool(SearchCreaturesCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_creatures",
            "List creatures (monsters and animals) near the player (within 100 metres), nearest first. Returns CSV with columns Name, Distance (m), Direction. Use for questions like 'are there any monsters nearby?'.",
            new[] { new ToolParameter("query", "Optional text to match against creature names, e.g. 'greydwarf'.", Required: false) });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");

            var creatures = _command.Execute(query)
                .Take(ToolLimits.MaxResults)
                .Select(creature => new CreatureSummary(creature.Name, creature.Distance, creature.Direction))
                .ToArray();

            return new ToolSuccess<SearchCreaturesResult>(
                new SearchCreaturesResult(creatures),
                FormatContent(query, creatures));
        }

        private static string FormatContent(string query, IReadOnlyList<CreatureSummary> creatures)
        {
            if (creatures.Count == 0)
            {
                return string.IsNullOrWhiteSpace(query)
                    ? "No creatures found nearby."
                    : $"No creatures matching '{query}' nearby.";
            }

            return Csv.Build(
                new[] { "Name", "Distance (m)", "Direction" },
                creatures.Select(creature => new[]
                {
                    creature.Name,
                    Number(creature.Distance),
                    creature.Direction,
                }));
        }

        private static string Number(float value)
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }
    }
}