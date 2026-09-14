using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Query
{
    /// <summary>Case-insensitive substring search over items in nearby containers.</summary>
    public sealed class StorageIndex
    {
        private readonly IStorageRepository _storage;

        public StorageIndex(IStorageRepository storage)
        {
            _storage = storage;
        }

        /// <summary>Containers holding at least one matching item, with only the matches kept.</summary>
        public IReadOnlyList<StorageContainer> Search(float radius, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<StorageContainer>();
            }

            var matches = new List<StorageContainer>();
            foreach (var container in _storage.GetNearbyContainers(radius))
            {
                var items = container.Items
                    .Where(item => item.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();

                if (items.Length > 0)
                {
                    matches.Add(new StorageContainer(container.Name, items));
                }
            }

            return matches;
        }
    }
}