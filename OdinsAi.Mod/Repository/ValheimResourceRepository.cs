using System;
using System.Collections.Generic;
using OdinsAi.Entity;
using UnityEngine;

namespace OdinsAi.Repository
{
    /// <summary>Finds resource nodes (ore deposits, pickables, trees) near the player.</summary>
    public sealed class ValheimResourceRepository : IResourceRepository
    {
        public IReadOnlyList<ResourceNode> GetNearbyResources(float radius)
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return Array.Empty<ResourceNode>();
            }

            var origin = player.transform.position;
            var resources = new List<ResourceNode>();

            Collect(origin, radius, "ore", resources, (MineRock5 rock) => rock.GetHoverName());
            Collect(origin, radius, "ore", resources, (MineRock rock) => rock.GetHoverName());
            Collect(origin, radius, "pickable", resources, (Pickable pickable) => pickable.GetHoverName());
            Collect(origin, radius, "tree", resources, (TreeBase tree) => tree.name);

            return resources;
        }

        private static void Collect<T>(
            Vector3 origin,
            float radius,
            string type,
            List<ResourceNode> resources,
            Func<T, string> nameOf)
            where T : Component
        {
            foreach (var component in UnityEngine.Object.FindObjectsOfType<T>())
            {
                if (component == null)
                {
                    continue;
                }

                var position = component.transform.position;
                var distance = Utils.DistanceXZ(origin, position);
                if (distance > radius)
                {
                    continue;
                }

                var delta = position - origin;
                resources.Add(new ResourceNode(
                    nameOf(component),
                    type,
                    distance,
                    OdinsAi.Query.Compass.Direction(delta.x, delta.z)));
            }
        }
    }
}