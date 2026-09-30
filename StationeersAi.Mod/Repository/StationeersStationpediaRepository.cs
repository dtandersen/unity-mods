using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI;
using StationeersAi.Entity;

namespace StationeersAi.Repository
{
    /// <summary>
    /// Reads the in-game encyclopedia (<c>Stationpedia.StationpediaPages</c>).
    /// Snapshotted on the calling (main) thread.
    /// </summary>
    public sealed class StationeersStationpediaRepository : IStationpediaRepository
    {
        private readonly List<StationpediaEntry> _entries = new List<StationpediaEntry>();

        public StationeersStationpediaRepository(IEnumerable<StationpediaPage> pages)
        {
            if (pages == null)
            {
                return;
            }

            foreach (var page in pages)
            {
                if (page == null || string.IsNullOrWhiteSpace(page.Key))
                {
                    continue;
                }

                _entries.Add(new StationpediaEntry(
                    Key: page.Key,
                    Title: string.IsNullOrWhiteSpace(page.Title) ? page.Key : page.Title,
                    Description: page.Description,
                    Text: string.IsNullOrWhiteSpace(page.Text) ? page.ConstructWithText : page.Text));
            }
        }

        public IReadOnlyList<StationpediaEntry> GetAll()
        {
            return _entries;
        }

        public StationpediaEntry GetByKey(string key)
        {
            return _entries.FirstOrDefault(entry => entry.Key == key);
        }
    }
}
