using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Command;

namespace OdinsAi.Tool
{
    /// <summary>Tool: list the world's bosses and whether they have been slain.</summary>
    public sealed class ListBossesTool : ITool
    {
        private readonly GetBossStatusCommand _command;

        public ListBossesTool(GetBossStatusCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "list_bosses",
            "List the world's bosses and whether they have been slain. Returns CSV with columns Boss, Defeated.",
            new ToolParameter[0]);

        public ToolResult Execute(ToolArguments arguments)
        {
            var bosses = _command.Execute();
            if (bosses == null)
            {
                return new ToolFailure("Boss data is not available yet.");
            }

            var summaries = bosses
                .OrderBy(boss => boss.Name, StringComparer.OrdinalIgnoreCase)
                .Select(boss => new BossSummary(boss.Name, boss.Defeated))
                .ToArray();

            var content = summaries.Length == 0
                ? "No bosses found."
                : Csv.Build(
                    new[] { "Boss", "Defeated" },
                    summaries.Select(boss => new[] { boss.Name, boss.Defeated ? "Yes" : "No" }));

            return new ToolSuccess<ListBossesResult>(new ListBossesResult(summaries), content);
        }
    }
}