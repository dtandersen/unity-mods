using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using StationeersAi.Entity;
using StructureEntity = StationeersAi.Entity.Structure;

namespace StationeersAi.Repository;

/// <summary>
/// Reads buildable structures from the game's prefab catalogue
/// (<c>Prefab.AllPrefabs</c>). Snapshotted on the calling (main) thread.
/// </summary>
public sealed class StationeersStructureRepository : IStructureRepository
{
    private readonly List<StructureEntity> _structures = new List<StructureEntity>();

    public StationeersStructureRepository(IEnumerable<Thing>? prefabs)
    {
        if (prefabs == null)
        {
            return;
        }

        foreach (var prefab in prefabs)
        {
            if (prefab == null || !(prefab is Assets.Scripts.Objects.Structure))
            {
                continue;
            }

            _structures.Add(new StructureEntity(
                Id: prefab.PrefabName,
                Name: StationeersItemRepository.SafeDisplayName(prefab),
                Description: SafeTooltip(prefab)));
        }
    }

    public IReadOnlyList<StructureEntity> GetAll()
    {
        return _structures;
    }

    public StructureEntity? GetById(string id)
    {
        return _structures.FirstOrDefault(structure => structure.Id == id);
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
