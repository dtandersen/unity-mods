using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using StationeersAi.Entity;
using ItemEntity = StationeersAi.Entity.Item;

namespace StationeersAi.Repository;

/// <summary>
/// Reads items from the game's prefab catalogue (<c>Prefab.AllPrefabs</c>).
/// This is world data, so the snapshot is materialised on the calling (main)
/// thread and cached until the catalogue changes.
/// </summary>
public sealed class StationeersItemRepository : IItemRepository
{
    private readonly List<ItemEntity> _items = new List<ItemEntity>();

    public StationeersItemRepository(IEnumerable<Thing>? prefabs)
    {
        if (prefabs == null)
        {
            return;
        }

        foreach (var prefab in prefabs)
        {
            if (prefab == null || !(prefab is DynamicThing))
            {
                continue;
            }

            var name = SafeDisplayName(prefab);
            _items.Add(new ItemEntity(
                Id: prefab.PrefabName,
                Name: name,
                Type: prefab.GetType().Name));
        }
    }

    public IReadOnlyList<ItemEntity> GetAll()
    {
        return _items;
    }

    public ItemEntity GetById(string id)
    {
        return _items.FirstOrDefault(item => item.Id == id);
    }

    internal static string SafeDisplayName(Thing prefab)
    {
        try
        {
            var name = prefab.DisplayName;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }
        catch (System.Exception)
        {
            // Prefab display names can depend on game state; fall back below.
        }

        return string.IsNullOrWhiteSpace(prefab.PrefabName) ? "Unknown" : prefab.PrefabName;
    }
}
