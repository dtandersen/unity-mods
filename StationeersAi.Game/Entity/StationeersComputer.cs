using System;
using System.Collections.Generic;
using System.Reflection;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using StationeersAi.Formatting;
using StationeersAi.Repository;

namespace StationeersAi.Entity;

/// <summary>
/// One live IC computer, built by <see cref="StationeersIcDevices"/>
/// from the motherboard it names. The screen reads and writes editor text on
/// that motherboard, the dropdown picks a housing from the motherboard's
/// holder list, and the buttons move text between the editor and the chip,
/// doing only what the game's own buttons do — press, and let the chip's
/// state tell how it went. Only housings on the computer's network appear in
/// the holder list. Runs on Unity's main thread: the burn compiles on the
/// chip and touches Unity state.
/// </summary>
public sealed class StationeersComputer : IComputer
{
    private readonly ProgrammableChipMotherboard _motherboard;

    private readonly string _name;

    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersComputer(ProgrammableChipMotherboard motherboard, string name, IMainThread threads, Logger log)
    {
        _motherboard = motherboard;
        _name = name;
        _threads = threads;
        _log = log;
    }

    public string Name => _name;

    public void Write(string code)
    {
        Press(() =>
        {
            _motherboard.SetSourceCode(code ?? string.Empty);
            try
            {
                _motherboard.SendUpdate();
            }
            catch (Exception ex)
            {
                _log.Info(ex.ToString());
            }
        }, "Editor write failed.");
    }

    public Program Read()
    {
        try
        {
            return _threads.RunSync(ReadInner);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            throw;
        }
    }

    public void Export(long device)
    {
        Press(() =>
        {
            SelectHolder(_motherboard, MatchNetworked(device));
            _motherboard.Export();
        }, "Export failed.");
    }

    public void Import(long device)
    {
        Press(() =>
        {
            SelectHolder(_motherboard, MatchNetworked(device));
            _motherboard.Import();
        }, "Import failed.");
    }

    public void Select(long device)
    {
        Press(() => SelectHolder(_motherboard, MatchNetworked(device)), "Select failed.");
    }

    public long Selected()
    {
        try
        {
            return _threads.RunSync(() =>
            {
                var housing = PointedHousing();
                return housing == null ? 0 : SafeId(housing);
            });
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            throw;
        }
    }

    private void Press(Action action, string fault)
    {
        try
        {
            _threads.RunSync(() =>
            {
                action();
                return true;
            });
        }
        catch (ThingNotFound)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            throw new InvalidOperationException(fault, ex);
        }
    }

    private Program ReadInner()
    {
        var source = _motherboard.GetSourceCode();
        var code = source != null ? source.ToString() : string.Empty;
        return new Program(code ?? string.Empty, CodeHash.Hash(code));
    }

    /// <summary>The housing under the dropdown, or null when it points at none. Throws when the dropdown cannot be read.</summary>
    private CircuitHousing? PointedHousing()
    {
        var dropdown = Dropdown(_motherboard);
        var selected = (int?)dropdown?.GetType().GetProperty("SelectedIndex")?.GetValue(dropdown, null);
        var holders = MotherboardHolders(_motherboard);
        if (holders == null)
        {
            throw new InvalidOperationException("Select read failed.");
        }

        if (!selected.HasValue || selected.Value < 0 || selected.Value >= holders.Count)
        {
            return null;
        }

        return holders[selected.Value] as CircuitHousing;
    }

    /// <summary>
    /// The holder index of the one carrying thing id <paramref name="device"/>
    /// inside the motherboard's holder list: only housings on the computer's
    /// network. Raises <see cref="ThingNotFound"/> with what was there when no
    /// holder carries that id, including when the network holds none.
    /// </summary>
    private int MatchNetworked(long device)
    {
        var holders = MotherboardHolders(_motherboard);
        if (holders == null)
        {
            throw new InvalidOperationException("Motherboard holder list is missing.");
        }

        var networked = new List<CircuitHousing>();
        foreach (var holder in holders)
        {
            var housing = holder as CircuitHousing;
            if (housing != null)
            {
                networked.Add(housing);
            }
        }

        for (var index = 0; index < networked.Count; index++)
        {
            var housing = networked[index];
            long id;
            try
            {
                id = housing.ReferenceId;
            }
            catch (Exception)
            {
                continue;
            }

            if (id == device)
            {
                return holders.IndexOf(housing);
            }
        }

        throw new ThingNotFound("Housing", device, " on this computer's network", Described(networked));
    }

    /// <summary>The motherboard's dropdown, read by reflection; the game keeps it private.</summary>
    private static object? Dropdown(object motherboard)
    {
        try
        {
            var field = motherboard.GetType().GetField("_dropdown", BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(motherboard);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// The motherboard's holder list: only housings on the computer's
    /// network. Read by reflection; the game keeps it private.
    /// </summary>
    private static IList<ICircuitHolder>? MotherboardHolders(ProgrammableChipMotherboard motherboard)
    {
        try
        {
            var field = motherboard.GetType().GetField("_circuitHolders", BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(motherboard) as IList<ICircuitHolder>;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Points the motherboard dropdown at one holder by index and refreshes
    /// its label. <c>SetSelectedIndex</c> alone only flips the backing field;
    /// the visible text updates in <c>ItemClicked</c>, which also pops the
    /// dropdown open, so the label is set straight instead.
    /// </summary>
    private void SelectHolder(ProgrammableChipMotherboard motherboard, int index)
    {
        var dropdown = Dropdown(motherboard);
        if (dropdown == null)
        {
            throw new InvalidOperationException("Motherboard dropdown is missing.");
        }

        var type = dropdown.GetType();
        type.GetMethod("SetSelectedIndex")?.Invoke(dropdown, new object[] { index });

        var items = type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(dropdown) as System.Collections.IList;
        var textMesh = type.GetField("_selectedTextMesh", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(dropdown);
        if (items == null || textMesh == null || index < 0 || index >= items.Count)
        {
            throw new InvalidOperationException("Motherboard dropdown is missing.");
        }

        var title = items[index]?.GetType().GetProperty("Title")?.GetValue(items[index], null) as string;
        textMesh.GetType().GetProperty("text")?.SetValue(textMesh, title ?? string.Empty, null);
    }

    /// <summary>The networked housings as <c>label #id</c>, so a missed burn shows what to retry.</summary>
    private static string Described(IReadOnlyList<CircuitHousing> housings)
    {
        var listed = new List<string>();
        foreach (var housing in housings)
        {
            var name = SafeName(housing);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var entry = $"{name} #{SafeId(housing)}";
            if (!listed.Contains(entry))
            {
                listed.Add(entry);
            }
        }

        return listed.Count == 0 ? "none" : string.Join(", ", listed.ToArray());
    }

    private static string? SafeName(CircuitHousing housing)
    {
        try
        {
            return housing.DisplayName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static long SafeId(CircuitHousing housing)
    {
        try
        {
            return housing.ReferenceId;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
