using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>
/// Reads the local player's state: name, position, world, and the contents of
/// their worn/carried slots (hands, suit, helmet, backpack, uniform,
/// toolbelt...). Tolerates a missing player (dedicated server, main menu):
/// everything is then empty/unknown. Snapshotted on the calling (main) thread.
/// </summary>
public sealed class StationeersPlayerContext : IPlayerContext
{
    private readonly List<ItemStack> _inventory = new List<ItemStack>();

    public StationeersPlayerContext(Human human)
    {
        if (human == null)
        {
            return;
        }

        Name = PlayerName(human);
        Status = PlayerStatus(human);

        AddSlot("Left hand", human.LeftHandSlot);
        AddSlot("Right hand", human.RightHandSlot);
        AddSlot("Uniform", human.UniformSlot);
        AddSlot("Suit", human.SuitSlot);
        AddSlot("Helmet", human.HelmetSlot);
        AddSlot("Glasses", human.GlassesSlot);
        AddSlot("Backpack", human.BackpackSlot);
        AddSlot("Toolbelt", human.ToolbeltSlot);
        AddSlot("Stomach", human.StomachSlot);
    }

    public string Name { get; }

    public PlayerStatus Status { get; }

    public IReadOnlyList<ItemStack> Inventory => _inventory;

    private void AddSlot(string fallbackName, Slot slot)
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
        _inventory.Add(new ItemStack(
            Id: occupant.PrefabName,
            Name: name,
            Quantity: QuantityText(occupant),
            Slot: SlotName(slot, fallbackName)));
    }

    private static DynamicThing SafeOccupant(Slot slot)
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

    private static string QuantityText(DynamicThing thing)
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

    private static PlayerStatus PlayerStatus(Human human)
    {
        var position = SafePosition(human);
        return new Entity.PlayerStatus(position.x, position.y, position.z, WorldName(), null, RoomOf(human), FacingOf(human));
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

    private static string WorldName()
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
