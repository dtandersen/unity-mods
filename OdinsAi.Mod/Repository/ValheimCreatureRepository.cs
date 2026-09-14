using System;
using System.Collections.Generic;
using OdinsAi.Entity;
using UnityEngine;

namespace OdinsAi.Repository
{
    /// <summary>Finds creatures near the local player.</summary>
    public sealed class ValheimCreatureRepository : ICreatureRepository
    {
        public IReadOnlyList<Creature> GetNearbyCreatures(float radius)
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return Array.Empty<Creature>();
            }

            var origin = player.transform.position;
            var creatures = new List<Creature>();

            foreach (var character in UnityEngine.Object.FindObjectsOfType<Character>())
            {
                if (character == null || character.IsPlayer() || character.IsTamed())
                {
                    continue;
                }

                var position = character.transform.position;
                var distance = Utils.DistanceXZ(origin, position);
                if (distance > radius)
                {
                    continue;
                }

                var delta = position - origin;
                creatures.Add(new Creature(
                    character.GetHoverName(),
                    distance,
                    OdinsAi.Query.Compass.Direction(delta.x, delta.z)));
            }

            return creatures;
        }
    }
}