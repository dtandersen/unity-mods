using System;
using System.Collections.Generic;
using System.Linq;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Query
{
    /// <summary>
    /// Search over Stationpedia pages. Matching is case-insensitive substring on
    /// the title (falling back to the key); order is not significant.
    /// </summary>
    public sealed class StationpediaIndex
    {
        private readonly IStationpediaRepository _pages;

        public StationpediaIndex(IStationpediaRepository pages)
        {
            _pages = pages;
        }

        public IReadOnlyList<StationpediaEntry> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<StationpediaEntry>();
            }

            return _pages.GetAll()
                .Where(entry => Matches(entry, query))
                .ToArray();
        }

        private static bool Matches(StationpediaEntry entry, string query)
        {
            return (entry.Title != null &&
                    entry.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (entry.Key != null &&
                    entry.Key.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
