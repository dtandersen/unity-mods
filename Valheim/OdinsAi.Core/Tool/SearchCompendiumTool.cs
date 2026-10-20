using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OdinsAi.Command;
using OdinsAi.Entity;

namespace OdinsAi.Tool
{
    /// <summary>
    /// Tool: search the in-game compendium (lore and Munin's hints) by topic or text.
    /// Returns CSV with a short preview; use <c>read_compendium</c> for the full page.
    /// </summary>
    public sealed class SearchCompendiumTool : ITool
    {
        private const int ExcerptLength = 140;

        private readonly SearchCompendiumCommand _command;

        public SearchCompendiumTool(SearchCompendiumCommand command)
        {
            _command = command;
        }

        public ToolDefinition Definition { get; } = new ToolDefinition(
            "search_compendium",
            "Search the in-game compendium of lore and hints by topic or text. Returns CSV with columns Topic, Excerpt. Use read_compendium to read a full page.",
            new[] { new ToolParameter("query", "Text to match against page topics and bodies, e.g. 'boar' or 'swamp'.") });

        public ToolResult Execute(ToolArguments arguments)
        {
            var query = arguments.Get("query");
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolFailure("Missing required argument 'query'.");
            }

            var summaries = _command.Execute(query)
                .Take(ToolLimits.MaxResults)
                .Select(entry => new CompendiumSummary(entry.Topic, Excerpt(entry.Text)))
                .ToArray();

            return new ToolSuccess<SearchCompendiumResult>(
                new SearchCompendiumResult(summaries),
                FormatContent(query, summaries));
        }

        private static string FormatContent(string query, IReadOnlyList<CompendiumSummary> summaries)
        {
            if (summaries.Count == 0)
            {
                return $"No compendium pages found for '{query}'.";
            }

            return Csv.Build(
                new[] { "Topic", "Excerpt" },
                summaries.Select(summary => new[] { summary.Topic, summary.Excerpt }));
        }

        private static string Excerpt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var collapsed = CollapseWhitespace(text).Trim();
            return collapsed.Length <= ExcerptLength
                ? collapsed
                : collapsed.Substring(0, ExcerptLength).TrimEnd() + "\u2026";
        }

        private static string CollapseWhitespace(string text)
        {
            var builder = new StringBuilder(text.Length);
            var pendingSpace = false;
            foreach (var c in text)
            {
                if (char.IsWhiteSpace(c))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}