using System;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using StationeersAi.Entity;
using StationeersAi.Repository;
using UnityEngine;

namespace StationeersAi.Repository;

/// <summary>
/// Reads container slots: belts, crates, and anything else holding things
/// in slots. Carried containers count too (unlike the loose-thing search),
/// so a worn mining belt answers as well as a dropped one. Snapshotted on
/// Unity's main thread; dying things are skipped, never fatal.
/// </summary>
public sealed class StationeersThingContents : IThingContents
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersThingContents(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
    }

    public IReadOnlyList<ThingContent> Read(string thing)
    {
        try
        {
            return _threads.RunSync(() => ReadInner(thing));
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
            return null;
        }
    }

    private static IReadOnlyList<ThingContent> ReadInner(string thing)
    {
        var matches = ThingMatching.Match(
            UnityEngine.Object.FindObjectsOfType<DynamicThing>(),
            item => SafeName(item),
            item => SafeId(item),
            thing);
        if (matches.Count != 1)
        {
            return null;
        }

        var contents = new List<ThingContent>();
        List<Slot> slots = null;
        try
        {
            slots = matches[0].Slots;
        }
        catch (Exception)
        {
        }

        if (slots == null)
        {
            return contents;
        }

        foreach (var slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            DynamicThing occupant = null;
            try
            {
                occupant = slot.Get();
            }
            catch (Exception)
            {
                continue;
            }

            if (occupant == null)
            {
                continue;
            }

            string name = null;
            try
            {
                name = occupant.DisplayName ?? occupant.PrefabName;
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            string slotName = null;
            try
            {
                slotName = slot.DisplayName;
            }
            catch (Exception)
            {
            }

            contents.Add(new ThingContent(slotName ?? slot.Type.ToString(), name, QuantityText(occupant)));
        }

        return contents;
    }

    private static string QuantityText(DynamicThing thing)
    {
        if (thing is IQuantity quantity)
        {
            try
            {
                return quantity.GetQuantityText();
            }
            catch (Exception)
            {
            }
        }

        return null;
    }

    private static string SafeName(DynamicThing thing)
    {
        try
        {
            return thing.DisplayName ?? thing.PrefabName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static long SafeId(DynamicThing thing)
    {
        try
        {
            return thing.ReferenceId;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
