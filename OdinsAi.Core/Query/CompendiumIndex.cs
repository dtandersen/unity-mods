using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>
    /// Search over the compendium. Matching is a case-insensitive substring on the
    /// topic or the body text, so a keyword found only inside a page still resolves.
    /// </summary>
    public sealed class CompendiumIndex
    {
        private readonly IPlayerContext _player;

        public CompendiumIndex(IPlayerContext player)
        {
            _player = player;
        }

        /// <summary>Pages whose topic or text matches <paramref name="query"/>, by topic.</summary>
        public IReadOnlyList<CompendiumEntry> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<CompendiumEntry>();
            }

            return _player.Compendium
                .Where(entry => Contains(entry.Topic, query) || Contains(entry.Text, query))
                .OrderBy(entry => entry.Topic, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        /// <summary>
        /// The page with the given topic. Falls back to a unique substring match so a
        /// topic returned by <see cref="Search"/> can always be read back.
        /// </summary>
        public CompendiumEntry Get(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return null;
            }

            var entries = _player.Compendium;
            return entries.FirstOrDefault(entry => string.Equals(entry.Topic, topic, StringComparison.OrdinalIgnoreCase))
                ?? entries
                    .Where(entry => Contains(entry.Topic, topic))
                    .OrderBy(entry => entry.Topic, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
        }

        private static bool Contains(string value, string query)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}