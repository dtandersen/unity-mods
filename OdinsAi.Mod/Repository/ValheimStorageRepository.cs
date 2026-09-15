using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using UnityEngine;

namespace OdinsAi.Repository
{
    /// <summary>
    /// Finds containers within a radius of the target player using a physics sphere
    /// query, and materialises their contents.
    /// </summary>
    public sealed class ValheimStorageRepository : IStorageRepository
    {
        private readonly Player _player;

        /// <param name="player">The player to search around.</param>
        public ValheimStorageRepository(Player player)
        {
            _player = player;
        }

        public IReadOnlyList<StorageContainer> GetNearbyContainers(float radius)
        {
            if (_player == null)
            {
                return Array.Empty<StorageContainer>();
            }

            var origin = _player.transform.position;
            var colliders = Physics.OverlapSphere(origin, radius);
            var seen = new HashSet<Container>();
            var containers = new List<StorageContainer>();

            foreach (var collider in colliders)
            {
                var container = collider != null ? collider.GetComponentInParent<Container>() : null;
                if (container == null || !seen.Add(container) || !container.IsOwner())
                {
                    continue;
                }

                var inventory = container.GetInventory();
                if (inventory == null)
                {
                    continue;
                }

                var items = inventory.GetAllItems().Select(ValheimItems.ToStack).ToArray();
                if (items.Length == 0)
                {
                    continue;
                }

                containers.Add(new StorageContainer(ValheimItemRepository.Localize(container.m_name), items));
            }

            return containers;
        }
    }
}