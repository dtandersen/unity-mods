using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OdinsAi.Command;
using OdinsAi.Entity;

namespace OdinsAi.Tool
{
    /// <summary>
    /// Tool: list dungeon entrances near the player, nearest first. Backed by the
    /// same loaded <c>DungeonGenerator</c> data the vanilla <c>printseeds</c> command uses.
    /// </summary>
    public sealed class SearchDungeonsTool : ITool
    {
        private readonly SearchDungeonsCommand _command;

        public SearchDungeonsTool(SearchDungeonsCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_dungeons",
            "List dungeon entrances near the player (within 500 metres), nearest first. Returns CSV with columns Name, Distance (m), Direction. Use for questions like 'what dungeons are nearby?'.",
            new[] { new ToolParameter("query", "Optional text to match against dungeon names, e.g. 'crypt'.", Required: false) });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");

            var dungeons = _command.Execute(query)
                .Take(ToolLimits.MaxResults)
                .Select(dungeon => new DungeonSummary(dungeon.Name, dungeon.Distance, dungeon.Direction))
                .ToArray();

            return new ToolSuccess<SearchDungeonsResult>(
                new SearchDungeonsResult(dungeons),
                FormatContent(query, dungeons));
        }

        private static string FormatContent(string query, IReadOnlyList<DungeonSummary> dungeons)
        {
            if (dungeons.Count == 0)
            {
                return string.IsNullOrWhiteSpace(query)
                    ? "No dungeons found nearby."
                    : $"No dungeons matching '{query}' nearby.";
            }

            return Csv.Build(
                new[] { "Name", "Distance (m)", "Direction" },
                dungeons.Select(dungeon => new[]
                {
                    dungeon.Name,
                    Number(dungeon.Distance),
                    dungeon.Direction,
                }));
        }

        private static string Number(float value)
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }
    }
}