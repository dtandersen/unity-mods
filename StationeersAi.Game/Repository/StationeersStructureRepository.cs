using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using StationeersAi.Game;
using StructureEntity = StationeersAi.Entity.Structure;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Repository;

/// <summary>
/// Reads buildable structures from the game's prefab catalogue. Asks the
/// provider for the current prefabs on every read and maps them on Unity's
/// main thread, so it holds no snapshot and never goes stale.
/// </summary>
public sealed class StationeersStructureRepository : IStructureRepository
{
    private readonly IPrefabProvider _prefabs;
    private readonly IMainThread _executor;
    private readonly Logger _log;

    public StationeersStructureRepository(IPrefabProvider prefabs, IMainThread executor, Logger log)
    {
        _prefabs = prefabs ?? throw new ArgumentNullException(nameof(prefabs));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public IReadOnlyList<StructureEntity> GetAll()
    {
        try
        {
            return _executor.RunSync(ReadAll);
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
            return new StructureEntity[0];
        }
    }

    public StructureEntity? GetById(string id)
    {
        return GetAll().FirstOrDefault(structure => structure.Id == id);
    }

    private IReadOnlyList<StructureEntity> ReadAll()
    {
        var structures = new List<StructureEntity>();
        var prefabs = _prefabs.GetPrefabs();
        if (prefabs == null)
        {
            return structures;
        }

        foreach (var prefab in prefabs)
        {
            if (prefab == null || !(prefab is Assets.Scripts.Objects.Structure))
            {
                continue;
            }

            structures.Add(new StructureEntity(
                Id: prefab.PrefabName,
                Name: StationeersItemRepository.SafeDisplayName(prefab),
                Description: SafeTooltip(prefab)));
        }

        return structures;
    }

    private static string? SafeTooltip(Thing prefab)
    {
        try
        {
            var tooltip = prefab.ToTooltip();
            if (string.IsNullOrWhiteSpace(tooltip))
            {
                return null;
            }

            tooltip = tooltip.Trim();
            return tooltip.Length > 300 ? tooltip.Substring(0, 300) + "..." : tooltip;
        }
        catch (System.Exception)
        {
            return null;
        }
    }
}
