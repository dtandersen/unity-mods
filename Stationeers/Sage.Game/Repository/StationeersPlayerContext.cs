using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Sage.Entity;
using Sage.Game;
using Logger = Crowbus.Common.Logger;

namespace Sage.Repository;

/// <summary>
/// Reads the local player's state live on every access: name, position,
/// world, and the contents of their worn/carried slots (hands, suit,
/// helmet, backpack, uniform, toolbelt...). Tolerates a missing player
/// (dedicated server, main menu): everything is then empty/unknown. Reads
/// run on Unity's main thread through the executor.
/// </summary>
public sealed class StationeersPlayerContext : IPlayerContext
{
    private readonly IHumanProvider _humans;
    private readonly IMainThread _executor;
    private readonly Logger _log;

    public StationeersPlayerContext(IHumanProvider humans, IMainThread executor, Logger log)
    {
        _humans = humans;
        _executor = executor;
        _log = log;
    }

    public string? Name => Read(h => h == null ? null : PlayerName(h), (string?)null);

    public Player? Status => Read(h => h == null ? null : PlayerStatus(h), (Player?)null);

    public IReadOnlyList<ItemStack> Inventory => Read(ReadInventory, new ItemStack[0]);

    private T Read<T>(Func<Human?, T> map, T empty)
    {
        try
        {
            return _executor.RunSync(() => map(_humans.GetHuman()));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return empty;
        }
    }

    private static IReadOnlyList<ItemStack> ReadInventory(Human? human)
    {
        var inventory = new List<ItemStack>();
        if (human == null)
        {
            return inventory;
        }

        AddSlot(inventory, "Left hand", human.LeftHandSlot, "Left");
        AddSlot(inventory, "Right hand", human.RightHandSlot, "Right");
        AddSlot(inventory, "Uniform", human.UniformSlot, null);
        AddSlot(inventory, "Suit", human.SuitSlot, null);
        AddSlot(inventory, "Helmet", human.HelmetSlot, null);
        AddSlot(inventory, "Glasses", human.GlassesSlot, null);
        AddSlot(inventory, "Backpack", human.BackpackSlot, null);
        AddSlot(inventory, "Toolbelt", human.ToolbeltSlot, null);
        AddSlot(inventory, "Stomach", human.StomachSlot, null);
        return inventory;
    }

    private static void AddSlot(List<ItemStack> inventory, string fallbackName, Slot slot, string? hand)
    {
        if (slot == null)
        {
            return;
        }

        var occupant = SafeOccupant(slot);
        if (occupant == null)
        {
            return;
        }

        var name = StationeersItemRepository.SafeDisplayName(occupant);
        var label = SlotName(slot, fallbackName);
        inventory.Add(new ItemStack(
            PrefabName: occupant.PrefabName,
            Name: name,
            Quantity: QuantityText(occupant),
            Slot: label,
            Hand: hand));
        AddNested(inventory, label, occupant);
    }

    /// <summary>
    /// Into worn/carried containers (belt, backpack) at any depth: ore and
    /// tools inside them list under the parent slot. Depth-capped and
    /// skips empties, never fatal.
    /// </summary>
    private static void AddNested(List<ItemStack> inventory, string parentSlot, DynamicThing holder)
    {
        AddNested(inventory, parentSlot, holder, 0);
    }

    private static void AddNested(List<ItemStack> inventory, string parentSlot, DynamicThing holder, int depth)
    {
        if (depth > 4)
        {
            return;
        }

        List<Slot>? slots = null;
        try
        {
            slots = holder.Slots;
        }
        catch (System.Exception)
        {
            return;
        }

        if (slots == null)
        {
            return;
        }

        foreach (var slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            var nested = SafeOccupant(slot);
            if (nested == null)
            {
                continue;
            }

            inventory.Add(new ItemStack(
                PrefabName: nested.PrefabName,
                Name: StationeersItemRepository.SafeDisplayName(nested),
                Quantity: QuantityText(nested),
                Slot: parentSlot));
            AddNested(inventory, parentSlot, nested, depth + 1);
        }
    }

    private static DynamicThing? SafeOccupant(Slot slot)
    {
        try
        {
            return slot.Get();
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static string SlotName(Slot slot, string fallback)
    {
        try
        {
            var name = slot.DisplayName;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }
        catch (System.Exception)
        {
        }

        return fallback;
    }

    private static string? QuantityText(DynamicThing thing)
    {
        if (thing is IQuantity quantity)
        {
            try
            {
                return quantity.GetQuantityText();
            }
            catch (System.Exception)
            {
            }
        }

        return null;
    }

    private static string PlayerName(Human human)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(human.CustomName))
            {
                return human.CustomName;
            }

            var display = human.DisplayName;
            if (!string.IsNullOrWhiteSpace(display))
            {
                return display;
            }
        }
        catch (System.Exception)
        {
        }

        return "Engineer";
    }

    private static Player PlayerStatus(Human human)
    {
        var position = SafePosition(human);
        return new Entity.Player(position.x, position.y, position.z, WorldName(), null, RoomOf(human), FacingOf(human));
    }

    /// <summary>Compass heading in degrees (0 north, 90 east), or null.</summary>
    private static double? FacingOf(Human human)
    {
        try
        {
            if (human == null)
            {
                return null;
            }

            var forward = human.EntityForward;
            var degrees = System.Math.Atan2(forward.x, -forward.z) * 180.0 / System.Math.PI;
            if (degrees < 0)
            {
                degrees += 360.0;
            }

            return degrees;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static long? RoomOf(Human human)
    {
        try
        {
            if (human == null)
            {
                return null;
            }

            var room = human.Room;
            return room != null ? (long?)room.RoomId : null;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static UnityEngine.Vector3 SafePosition(Human human)
    {
        try
        {
            return human.ThingTransformPosition;
        }
        catch (System.Exception)
        {
            return UnityEngine.Vector3.zero;
        }
    }

    private static string? WorldName()
    {
        try
        {
            return WorldManager.CurrentWorldName;
        }
        catch (System.Exception)
        {
            return null;
        }
    }
}
