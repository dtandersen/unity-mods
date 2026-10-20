using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using BuildEntryEntity = Sage.Entity.BuildEntry;
using BuildStepEntity = Sage.Entity.BuildStep;

namespace Sage.Repository;

/// <summary>
/// Reads how a prefab builds from its build states. Each state names up to
/// two entries with quantities, following the game's own rule
/// (<c>Item.GetToolString</c>): entries that carry quantities are materials
/// you spend, the rest are tools you hold (wrench, welding torch). Every
/// entry keeps its prefab name, display name, and quantity, so Sage can link
/// a requirement back to the catalogue. The game spends the next state's
/// entries and then moves on, so callers choose where reading starts: the
/// first state for the whole build, past the current state for what
/// finishing still needs.
/// </summary>
internal static class BuildMaterials
{
    /// <summary>One step per state from the given 0-based state on, numbered 1-based, skipping states with nothing on them.</summary>
    internal static IReadOnlyList<BuildStepEntity> StepsFrom(IList<BuildState> states, int first)
    {
        var steps = new List<BuildStepEntity>();
        if (states == null)
        {
            return steps;
        }

        for (var state = Math.Max(first, 0); state < states.Count; state++)
        {
            var tool = states[state]?.Tool;
            var entries = Entries(tool?.ToolEntry, tool?.EntryQuantity ?? 0)
                .Concat(Entries(tool?.ToolEntry2, tool?.EntryQuantity2 ?? 0))
                .ToArray();
            if (entries.Length > 0)
            {
                steps.Add(new BuildStepEntity(state + 1, entries));
            }
        }

        return steps;
    }

    /// <summary>
    /// One entry read the game's way: quantity-carrying items come back with
    /// a quantity to spend, tools come back quantity-less. Dead entries (no
    /// item or nothing to spend) come back empty.
    /// </summary>
    private static IReadOnlyList<BuildEntryEntity> Entries(Item? entry, int quantity)
    {
        if (entry == null)
        {
            return Array.Empty<BuildEntryEntity>();
        }

        var name = StationeersItemRepository.SafeDisplayName(entry);
        var id = string.IsNullOrWhiteSpace(entry.PrefabName) ? name : entry.PrefabName;
        if (entry is IQuantity)
        {
            return quantity <= 0
                ? Array.Empty<BuildEntryEntity>()
                : new[] { new BuildEntryEntity(id, string.IsNullOrWhiteSpace(name) ? id : name, quantity) };
        }

        return new[] { new BuildEntryEntity(id, string.IsNullOrWhiteSpace(name) ? id : name, null) };
    }
}
