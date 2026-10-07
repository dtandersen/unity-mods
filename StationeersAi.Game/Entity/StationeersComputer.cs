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
/// holder list, and the buttons move text between the editor and the chip —
/// <c>Export</c> burns and verifies the burn by comparing code hashes,
/// <c>Import</c> pulls chip text back. Only housings on the computer's
/// network appear in the holder list. Runs on Unity's main thread: the burn
/// compiles on the chip and touches Unity state.
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

    public IcWriteResult Write(string code, string chipHash)
    {
        try
        {
            return _threads.RunSync(() => WriteInner(code ?? string.Empty, chipHash));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Editor write failed.");
        }
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

    public IcWriteResult Export(long device)
    {
        try
        {
            return _threads.RunSync(() => ExportInner(device));
        }
        catch (ThingNotFound notFound)
        {
            return new IcWriteResult(false, null, 0, notFound.Message);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Export failed.");
        }
    }

    public IcWriteResult Import(long device)
    {
        try
        {
            return _threads.RunSync(() => ImportInner(device));
        }
        catch (ThingNotFound notFound)
        {
            return new IcWriteResult(false, null, 0, notFound.Message);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Import failed.");
        }
    }

    public IcWriteResult Select(long device)
    {
        try
        {
            return _threads.RunSync(() => SelectInner(device));
        }
        catch (ThingNotFound notFound)
        {
            return new IcWriteResult(false, null, 0, notFound.Message);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Select failed.");
        }
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

    private IcWriteResult WriteInner(string code, string chipHash)
    {
        var pointed = PointedHousing();
        var chip = pointed == null ? null : ChipProgram(pointed);
        var refusal = WriteGuard.Refusal(
            pointed == null ? null : SafeName(pointed),
            chip == null ? string.Empty : chip.Hash,
            chipHash);
        if (refusal != null)
        {
            return new IcWriteResult(false, null, 0, refusal);
        }

        _motherboard.SetSourceCode(code);
        try
        {
            _motherboard.SendUpdate();
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
        }

        return new IcWriteResult(true, _name, code.Split('\n').Length, null);
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

    /// <summary>The program burned on the chip of one housing.</summary>
    private Program ChipProgram(CircuitHousing housing)
    {
        var chip = HousingChip(housing);
        string code;
        try
        {
            var source = chip?.GetSourceCode();
            code = source != null ? source.ToString() : string.Empty;
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            throw new InvalidOperationException("Select read failed.");
        }

        code = code ?? string.Empty;
        return new Program(code, CodeHash.Hash(code));
    }

    private IcWriteResult SelectInner(long device)
    {
        var selection = MatchNetworked(_motherboard, device);
        if (selection.Error != null)
        {
            return selection.Error;
        }

        try
        {
            SelectHolder(_motherboard, selection.Index);
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Select failed.");
        }

        var housing = selection.Housing;
        if (housing == null)
        {
            return new IcWriteResult(false, null, 0, "Select failed.");
        }

        var name = SafeName(housing);
        return new IcWriteResult(true, name ?? "housing", 0, null);
    }

    private IcWriteResult ImportInner(long device)
    {
        var selection = MatchNetworked(_motherboard, device);
        if (selection.Error != null)
        {
            return selection.Error;
        }

        try
        {
            SelectHolder(_motherboard, selection.Index);
            _motherboard.Import();
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Import failed.");
        }

        string text;
        try
        {
            var source = _motherboard.GetSourceCode();
            text = source != null ? source.ToString() : string.Empty;
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Editor read failed.");
        }

        return new IcWriteResult(true, _name, text.Split('\n').Length, null);
    }

    private IcWriteResult ExportInner(long device)
    {
        string text;
        try
        {
            var source = _motherboard.GetSourceCode();
            text = source != null ? source.ToString() : string.Empty;
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Editor read failed.");
        }

        if (string.IsNullOrEmpty(text))
        {
            return new IcWriteResult(false, null, 0, "Editor is empty; nothing to export.");
        }

        var selection = MatchNetworked(_motherboard, device);
        if (selection.Error != null)
        {
            return selection.Error;
        }

        var target = selection.Housing;
        if (target == null)
        {
            return new IcWriteResult(false, null, 0, "Export failed.");
        }

        try
        {
            SelectHolder(_motherboard, selection.Index);
            _motherboard.Export();
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return new IcWriteResult(false, null, 0, "Export failed.");
        }

        var chip = HousingChip(target);
        if (chip != null && chip.CompilationError)
        {
            var type = ChipErrorReading.CompileErrorType(chip) ?? "compile error";
            var line = ChipErrorReading.CompileErrorLine(chip);
            return new IcWriteResult(false, null, 0, $"Chip reports {type}" + (line == null ? "." : $" at line {line}."));
        }

        string? burned = null;
        try
        {
            if (chip != null)
            {
                burned = chip.GetSourceCode().ToString();
            }
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
        }

        if (burned == null || CodeHash.Hash(burned) != CodeHash.Hash(text))
        {
            return new IcWriteResult(false, null, 0, "Chip hash does not match the editor.");
        }

        return new IcWriteResult(true, SafeName(target) ?? "housing", text.Split('\n').Length, null);
    }

    /// <summary>
    /// Finds the holder carrying thing id <paramref name="device"/> inside
    /// the motherboard's holder list: only housings on the computer's
    /// network. Carries the holder index for the dropdown, and refuses when
    /// no holder carries that id.
    /// </summary>
    private sealed class NetworkSelection
    {
        public int Index;
        public CircuitHousing? Housing;
        public IcWriteResult? Error;
    }

    private NetworkSelection MatchNetworked(ProgrammableChipMotherboard motherboard, long device)
    {
        var selection = new NetworkSelection();
        var holders = MotherboardHolders(motherboard);
        if (holders == null)
        {
            selection.Error = new IcWriteResult(false, null, 0, "Export failed.");
            return selection;
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

        if (networked.Count == 0)
        {
            selection.Error = new IcWriteResult(false, null, 0, "No housings on this computer's network.");
            return selection;
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
                selection.Index = holders.IndexOf(housing);
                selection.Housing = housing;
                return selection;
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

    private static ProgrammableChip? HousingChip(CircuitHousing housing)
    {
        try
        {
            return housing?._ProgrammableChipSlot?.Get<ProgrammableChip>();
        }
        catch (Exception)
        {
            return null;
        }
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
