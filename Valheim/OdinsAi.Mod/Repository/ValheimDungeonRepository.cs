using System;
using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Query;
using UnityEngine;

namespace OdinsAi.Repository
{
    /// <summary>
    /// Finds dungeon entrances near the target player using the same loaded
    /// <c>DungeonGenerator</c> components the vanilla <c>printseeds</c> command uses.
    /// </summary>
    public sealed class ValheimDungeonRepository : IDungeonRepository
    {
        private readonly Player _player;

        /// <param name="player">The player to search around.</param>
        public ValheimDungeonRepository(Player player)
        {
            _player = player;
        }

        public IReadOnlyList<Dungeon> GetNearbyDungeons(float radius)
        {
            if (_player == null)
            {
                return Array.Empty<Dungeon>();
            }

            var origin = _player.transform.position;
            var dungeons = new List<Dungeon>();

            foreach (var generator in UnityEngine.Object.FindObjectsOfType<DungeonGenerator>())
            {
                if (generator == null)
                {
                    continue;
                }

                var position = generator.transform.position;
                var distance = Utils.DistanceXZ(origin, position);
                if (distance > radius)
                {
                    continue;
                }

                var delta = position - origin;
                dungeons.Add(new Dungeon(
                    CleanName(generator.name),
                    distance,
                    OdinsAi.Query.Compass.Direction(delta.x, delta.z)));
            }

            return dungeons;
        }

        private static string CleanName(string name)
        {
            return string.IsNullOrEmpty(name)
                ? name
                : name.Replace("(Clone)", string.Empty).Trim();
        }
    }
}