using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using StationeersAi.Entity;
using StationeersAi.Game;
using ItemEntity = StationeersAi.Entity.Item;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Repository;

/// <summary>
/// Reads items from the game's prefab catalogue. Asks the provider for the
/// current prefabs on every read and maps them on Unity's main thread, so
/// it holds no snapshot and never goes stale.
/// </summary>
public sealed class StationeersItemRepository : IItemRepository
{
    private readonly IPrefabProvider _prefabs;
    private readonly IMainThread _executor;
    private readonly Logger _log;

    public StationeersItemRepository(IPrefabProvider prefabs, IMainThread executor, Logger log)
    {
        _prefabs = prefabs;
        _executor = executor;
        _log = log;
    }

    public IReadOnlyList<ItemEntity> GetAll()
    {
        try
        {
            return _executor.RunSync(ReadAll);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new ItemEntity[0];
        }
    }

    public ItemEntity GetById(string id)
    {
        return GetAll().FirstOrDefault(item => item.Id == id);
    }

    private IReadOnlyList<ItemEntity> ReadAll()
    {
        var items = new List<ItemEntity>();
        var prefabs = _prefabs.GetPrefabs();
        if (prefabs == null)
        {
            return items;
        }

        foreach (var prefab in prefabs)
        {
            if (prefab == null || !(prefab is DynamicThing))
            {
                continue;
            }

            var name = SafeDisplayName(prefab);
            items.Add(new ItemEntity(
                Id: prefab.PrefabName,
                Name: name,
                Type: prefab.GetType().Name));
        }

        return items;
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
