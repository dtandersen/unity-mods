using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using BuildStepEntity = StationeersAi.Entity.BuildStep;
using IngredientEntity = StationeersAi.Entity.Ingredient;

namespace StationeersAi.Repository;

/// <summary>
/// Reads what construction consumes from a prefab's build states. Each state
/// names up to two entries with quantities, following the game's own rule
/// (<c>Item.GetToolString</c>): entries that carry quantities are materials
/// you spend, the rest are tools you hold (wrench, welding torch). The game
/// spends the next state's entries and then moves on, so callers choose
/// where reading starts: the first state for the whole build, past the
/// current state for what finishing still needs.
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
            var step = Step(state + 1, tool?.ToolEntry, tool?.EntryQuantity ?? 0, tool?.ToolEntry2, tool?.EntryQuantity2 ?? 0);
            if (step != null)
            {
                steps.Add(step);
            }
        }

        return steps;
    }

    private static BuildStepEntity? Step(int number, Item? first, int firstQuantity, Item? second, int secondQuantity)
    {
        var tools = Entries(first, firstQuantity)
            .Concat(Entries(second, secondQuantity))
            .Where(entry => entry.Amount == null)
            .Select(entry => entry.Item)
            .ToArray();
        var materials = Entries(first, firstQuantity)
            .Concat(Entries(second, secondQuantity))
            .Where(entry => entry.Amount != null)
            .Select(entry => new IngredientEntity(entry.Item, entry.Amount!.Value))
            .ToArray();

        if (tools.Length == 0 && materials.Length == 0)
        {
            return null;
        }

        return new BuildStepEntity(
            number,
            tools.Length == 0 ? null : string.Join(", ", tools),
            materials);
    }

    /// <summary>
    /// One entry read the game's way: quantity-carrying items come back with
    /// an amount to spend, tools come back amount-less. Dead entries (no item
    /// or nothing to spend) come back empty.
    /// </summary>
    private static IReadOnlyList<(string Id, string Item, double? Amount)> Entries(Item? entry, int quantity)
    {
        if (entry == null)
        {
            return Array.Empty<(string, string, double?)>();
        }

        var name = StationeersItemRepository.SafeDisplayName(entry);
        var id = string.IsNullOrWhiteSpace(entry.PrefabName) ? name : entry.PrefabName;
        if (entry is IQuantity && quantity > 0)
        {
            return new[] { (id, string.IsNullOrWhiteSpace(name) ? id : name, (double?)quantity) };
        }

        if (entry is IQuantity)
        {
            return Array.Empty<(string, string, double?)>();
        }

        return new[] { (id, string.IsNullOrWhiteSpace(name) ? id : name, (double?)null) };
    }
}
