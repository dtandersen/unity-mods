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

            // Ore deposits use MineRock5 (multi-hit deposits); some props use the
            // single-piece MineRock. Their m_name is empty, so name them by what they
            // drop (e.g. "Tin ore").
            Collect(origin, radius, "ore", resources, (MineRock5 rock) => OreName(rock.m_dropItems, rock.GetHoverName()));
            Collect(origin, radius, "ore", resources, (MineRock rock) => OreName(rock.m_dropItems, rock.GetHoverName()));
            Collect(origin, radius, "pickable", resources, (Pickable pickable) => pickable.GetHoverName());
            Collect(origin, radius, "tree", resources, (TreeBase tree) => tree.name);

            return resources;
        }

        private static string OreName(DropTable drops, string hover)
        {
            if (drops != null)
            {
                var items = drops.GetDropListItems();
                if (items != null && items.Count > 0)
                {
                    var shared = items[0] != null ? items[0].m_shared : null;
                    if (shared != null && !string.IsNullOrWhiteSpace(shared.m_name))
                    {
                        return ValheimItemRepository.Localize(shared.m_name);
                    }
                }
            }

            return string.IsNullOrWhiteSpace(hover) ? null : ValheimItemRepository.Localize(hover);
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

                var candidate = nameOf(component);
                var name = string.IsNullOrWhiteSpace(candidate)
                    ? CleanName(component.name)
                    : candidate;

                if (type != "tree")
                {
                    Debug.Log($"[OdinsAi] {type}: gameObject='{component.name}' -> '{name}' @{distance:0}m");
                }

                var delta = position - origin;
                resources.Add(new ResourceNode(
                    name,
                    type,
                    distance,
                    OdinsAi.Query.Compass.Direction(delta.x, delta.z)));
            }
        }

        private static string CleanName(string name)
        {
            return string.IsNullOrEmpty(name)
                ? name
                : name.Replace("(Clone)", string.Empty).Trim();
        }
    }
}