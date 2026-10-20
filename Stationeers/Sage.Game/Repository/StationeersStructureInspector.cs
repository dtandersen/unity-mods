using System;
using System.Linq;
using Sage.Core;
using Sage.Game;
using Logger = Sage.Core.Logger;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Reagents;
using Sage.Entity;
using Sage.Formatting;
using Sage.Repository;
using UnityEngine;

namespace Sage.Repository;

/// <summary>
/// Describes one live structure: on/off state, every readable logic setting
/// with its live value and writability, and slots with occupants.
/// Snapshotted on Unity's main thread; unreadable entries are skipped,
/// never fatal.
/// </summary>
public sealed class StationeersStructureInspector : IStructureInspector
{
    private readonly IMainThread _threads;

    private readonly Logger _log;

    public StationeersStructureInspector(IMainThread mainThread, Logger log)
    {
        _threads = mainThread;
        _log = log;
    }
    public StructureDetails? Describe(string? structure)
    {
        try
        {
            return _threads.RunSync(() => DescribeInner(structure));
        }
        catch (Exception ex)
        {
            _log.Info(ex.ToString());
            return null;
        }
    }

    private StructureDetails? DescribeInner(string? structure)
    {
        var matches = ThingMatching.Match(
            UnityEngine.Object.FindObjectsOfType<Assets.Scripts.Objects.Structure>(),
            item => SafeName(item),
            item => item.ReferenceId,
            structure);
        if (matches.Count != 1)
        {
            return null;
        }

        var target = matches[0];
        var wired = target as Device;
        var connections = ReadConnections(target);

        bool? isOn = null;
        try
        {
            if (wired != null && wired.HasOnOffState)
            {
                isOn = wired.OnOff;
            }
        }
        catch (Exception)
        {
        }

        return new StructureDetails(
            SafeName(target),
            SafeType(target),
            SafeId(target),
            connections,
            isOn,
            ReadLogic(target as ILogicable),
            ReadSlots(target as ILogicable),
            ReadHousingDevices(target),
            ReadChip(target),
            ReadStorage(target),
            ReadBuild(target as Assets.Scripts.Objects.Structure));
    }

    private static List<NetworkConnection> ReadConnections(Assets.Scripts.Objects.Structure? structure)
    {
        var connections = new List<NetworkConnection>();
        if (structure == null)
        {
            return connections;
        }

        var wired = structure as Device;
        if (wired != null)
        {
            AddCableConnections(connections, wired);
            AddPipeConnections(connections, structure, wired);
            AddChuteConnections(connections, structure, wired);
        }
        else
        {
            AddPipeConnections(connections, structure, null);
            AddChuteConnections(connections, structure, null);
        }

        return connections;
    }

    private static void AddCableConnections(List<NetworkConnection> connections, Device wired)
    {
        try
        {
            var power = wired.PowerCables;
            if (power != null)
            {
                for (var i = 0; i < power.Length; i++)
                {
                    try
                    {
                        var network = power[i] != null ? power[i].CableNetwork : null;
                        if (network != null)
                        {
                            connections.Add(new NetworkConnection("power", i, network.ReferenceId));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        try
        {
            var data = wired.DataCables;
            if (data != null)
            {
                for (var i = 0; i < data.Length; i++)
                {
                    try
                    {
                        var network = data[i] != null ? data[i].CableNetwork : null;
                        if (network != null)
                        {
                            connections.Add(new NetworkConnection("data", i, network.ReferenceId));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Every pipe mouth: a lone pipe joins one network; a device with
    /// mouths reports input, input2, output, output2 in that order
    /// (output2 often carries liquids, e.g. the furnace liquid out).
    /// Anything else on pipe networks joins by network id.
    /// </summary>
    private static void AddPipeConnections(List<NetworkConnection> connections, Assets.Scripts.Objects.Structure structure, Device? wired)
    {
        try
        {
            var pipe = structure as Pipe;
            if (pipe != null)
            {
                try
                {
                    var own = pipe.PipeNetwork;
                    if (own != null)
                    {
                        connections.Add(new NetworkConnection("pipe", 0, own.ReferenceId));
                    }
                }
                catch (Exception)
                {
                }

                return;
            }
        }
        catch (Exception)
        {
        }

        try
        {
            var io = structure as DeviceInputOutput;
            if (io != null)
            {
                AddPipe(connections, "input", 0, SafePipeId(() => io.InputNetwork));
                AddPipe(connections, "input2", 1, SafePipeId(() => io.InputNetwork2));
                AddPipe(connections, "output", 2, SafePipeId(() => io.OutputNetwork));
                AddPipe(connections, "output2", 3, SafePipeId(() => io.OutputNetwork2));
                return;
            }
        }
        catch (Exception)
        {
        }

        if (wired == null)
        {
            return;
        }

        try
        {
            var joined = wired.ConnectedPipeNetworks;
            if (joined == null)
            {
                return;
            }

            var index = 0;
            foreach (var network in joined)
            {
                try
                {
                    if (network == null)
                    {
                        continue;
                    }

                    var id = network.ReferenceId;
                    if (connections.Any(c => c.Kind == "pipe" && c.NetworkId == id))
                    {
                        continue;
                    }

                    connections.Add(new NetworkConnection("pipe", index++, id));
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private static void AddPipe(List<NetworkConnection> connections, string port, int index, long? networkId)
    {
        if (networkId.HasValue && networkId.Value != 0)
        {
            connections.Add(new NetworkConnection("pipe", index, networkId.Value, port));
        }
    }

    private static long? SafePipeId(Func<object?> get)
    {
        try
        {
            var network = get() as Assets.Scripts.Networks.PipeNetwork;
            return network == null ? (long?)null : network.ReferenceId;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void AddChuteConnections(List<NetworkConnection> connections, Assets.Scripts.Objects.Structure structure, Device? wired)
    {
        try
        {
            var chute = structure as Chute;
            if (chute != null)
            {
                try
                {
                    var own = chute.ChuteNetwork;
                    if (own != null)
                    {
                        connections.Add(new NetworkConnection("chute", 0, own.ReferenceId));
                    }
                }
                catch (Exception)
                {
                }

                return;
            }
        }
        catch (Exception)
        {
        }

        if (wired == null)
        {
            return;
        }

        try
        {
            var joined = wired.ConnectedChuteNetworks;
            if (joined == null)
            {
                return;
            }

            var index = 0;
            foreach (var network in joined)
            {
                try
                {
                    if (network == null)
                    {
                        continue;
                    }

                    connections.Add(new NetworkConnection("chute", index++, network.ReferenceId));
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// How far construction of the placed structure has come, with one step
    /// per unfinished state after the current one. Null when the game reports
    /// no stages. Follows the game's own rule: a state that can manufacture
    /// counts as completed, so an operational printer with an optional mod
    /// pending reports complete.
    /// </summary>
    private static BuildProgress? ReadBuild(Assets.Scripts.Objects.Structure? structure)
    {
        try
        {
            var states = structure?.BuildStates;
            if (states == null || states.Count == 0)
            {
                return null;
            }

            try
            {
                if (structure!.IsStructureCompleted)
                {
                    return new BuildProgress(
                        states.Count,
                        states.Count,
                        Array.Empty<BuildStep>());
                }
            }
            catch (Exception)
            {
            }

            var index = structure!.CurrentBuildStateIndex;
            if (index < 0)
            {
                index = 0;
            }

            if (index > states.Count - 1)
            {
                index = states.Count - 1;
            }

            return new BuildProgress(
                index + 1,
                states.Count,
                BuildMaterials.StepsFrom(states, index + 1));
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// The running chip's hover-visible state: error line and type, the
    /// display value, and a hash of its code (never the code itself).
    /// Null unless a chip is slotted.
    /// </summary>
    private static ChipState? ReadChip(Assets.Scripts.Objects.Structure? structure)
    {
        try
        {
            var housing = structure as CircuitHousing;
            var chip = housing?._ProgrammableChipSlot?.Get<ProgrammableChip>();
            if (chip == null)
            {
                return null;
            }

            string? errorType = null;
            string? errorLine = null;
            try
            {
                if (chip.CompilationError)
                {
                    errorType = ChipErrorReading.CompileErrorType(chip) ?? "compile error";
                    errorLine = ChipErrorReading.CompileErrorLine(chip);
                }
                else if (!string.IsNullOrEmpty(chip.ErrorTypeString) &&
                    !chip.ErrorTypeString.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    errorType = chip.ErrorTypeString;
                    errorLine = string.IsNullOrEmpty(chip.ErrorLineNumberString) ? null : chip.ErrorLineNumberString;
                }
            }
            catch (Exception)
            {
            }

            double? display = null;
            try
            {
                var logicable = structure as ILogicable;
                if (logicable != null && logicable.CanLogicRead(LogicType.Setting))
                {
                    display = logicable.GetLogicValue(LogicType.Setting);
                }
            }
            catch (Exception)
            {
            }

            string? hash = null;
            try
            {
                var source = chip.GetSourceCode();
                hash = CodeHash.Hash(source != null ? source.ToString() : string.Empty);
            }
            catch (Exception)
            {
            }

            return new ChipState(errorType, errorLine, display, hash);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static IReadOnlyList<HousingDeviceEntry>? ReadHousingDevices(Assets.Scripts.Objects.Structure? structure)
    {
        var holder = structure as ICircuitHolder;
        if (holder == null)
        {
            return null;
        }

        List<LogicBinding> bindings;
        try
        {
            bindings = holder.GetLogicBindings();
        }
        catch (Exception)
        {
            return null;
        }

        if (bindings == null)
        {
            return null;
        }

        var entries = new List<HousingDeviceEntry>();
        foreach (var binding in bindings)
        {
            if (binding == null || string.IsNullOrEmpty(binding.Header))
            {
                continue;
            }

            if (binding.Header == "db")
            {
                entries.Add(new HousingDeviceEntry("db", SafeName(structure) ?? "housing", SafeId(structure)));
                continue;
            }

            int index;
            if (!binding.Header.StartsWith("d") || !int.TryParse(binding.Header.Substring(1), out index))
            {
                continue;
            }

            Thing? thing = null;
            try
            {
                var logicable = holder.GetLogicableFromIndex(index);
                thing = logicable != null ? logicable.GetAsThing : null;
            }
            catch (Exception)
            {
            }

            if (thing == null)
            {
                var screwed = ScrewedIn(structure, index);
                if (screwed != null)
                {
                    entries.Add(new HousingDeviceEntry(binding.Header, screwed.Item1 + " (off network)", screwed.Item2));
                }
                else
                {
                    entries.Add(new HousingDeviceEntry(binding.Header, "empty", 0));
                }

                continue;
            }

            string? label = null;
            try
            {
                label = thing.DisplayName;
            }
            catch (Exception)
            {
            }

            long id = 0;
            try
            {
                id = thing.ReferenceId;
            }
            catch (Exception)
            {
            }

            entries.Add(new HousingDeviceEntry(binding.Header, label ?? "unknown", id));
        }

        return entries;
    }

    private static IReadOnlyList<StructureLogicValue> ReadLogic(ILogicable logicable)
    {
        var values = new List<StructureLogicValue>();
        if (logicable == null)
        {
            return values;
        }

        foreach (LogicType type in Enum.GetValues(typeof(LogicType)))
        {
            try
            {
                if (LogicBase.IsDeprecated(type) || !logicable.CanLogicRead(type))
                {
                    continue;
                }

                values.Add(new StructureLogicValue(type.ToString(), logicable.GetLogicValue(type), logicable.CanLogicWrite(type)));
            }
            catch (Exception)
            {
            }
        }

        return values;
    }

    private static IReadOnlyList<StructureSlotInfo> ReadSlots(ILogicable logicable)
    {
        var slots = new List<StructureSlotInfo>();
        if (logicable == null)
        {
            return slots;
        }

        int count;
        try
        {
            count = logicable.TotalSlots;
        }
        catch (Exception)
        {
            return slots;
        }

        for (var index = 0; index < count; index++)
        {
            Slot slot;
            try
            {
                slot = logicable.GetSlot(index);
            }
            catch (Exception)
            {
                continue;
            }

            if (slot == null)
            {
                continue;
            }

            string? name = null;
            try
            {
                name = slot.DisplayName;
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(name))
            {
                name = slot.Type.ToString();
            }

            string occupant = "empty";
            try
            {
                var held = slot.Get();
                if (held != null)
                {
                    occupant = StationeersItemRepository.SafeDisplayName(held) ?? held.PrefabName ?? "unknown";
                }
            }
            catch (Exception)
            {
            }

            slots.Add(new StructureSlotInfo(index, name, occupant));
        }

        return slots;
    }

    /// <summary>
    /// Internal reagents of fabricators (autolathe, electronics printer),
    /// furnaces, and anything else holding a mixture: the hopper contents
    /// stockpile tallies need. Every <c>Thing</c> carries a mixture, so this
    /// reads it straight off the structure. Null when the mixture itself is
    /// missing; empty when it exists but is unfilled.
    /// </summary>
    private static IReadOnlyList<ReagentAmount>? ReadStorage(Assets.Scripts.Objects.Structure? structure)
    {
        try
        {
            var thing = structure as Thing;
            var mixture = thing != null ? thing.ReagentMixture : null;
            if (mixture == null)
            {
                return null;
            }

            var amounts = new List<ReagentAmount>();
            foreach (var reagent in Reagent.AllReagentsSorted)
            {
                if (reagent == null)
                {
                    continue;
                }

                double quantity;
                try
                {
                    quantity = mixture.Get(reagent);
                }
                catch (Exception)
                {
                    continue;
                }

                if (quantity <= 0)
                {
                    continue;
                }

                string? name = null;
                try
                {
                    name = reagent.DisplayName;
                }
                catch (Exception)
                {
                }

                amounts.Add(new ReagentAmount(name ?? reagent.TypeName ?? "unknown", quantity));
            }

            return amounts;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>The thing screwed into a housing screw, even when the chip cannot see it.</summary>
    private static Tuple<string, long>? ScrewedIn(Assets.Scripts.Objects.Structure? structure, int index)
    {
        try
        {
            var housing = structure as CircuitHousing;
            if (housing == null || housing.Devices == null || index < 0 || index >= housing.Devices.Length)
            {
                return null;
            }

            var thing = housing.Devices[index]?.GetAsThing;
            if (thing == null)
            {
                return null;
            }

            return Tuple.Create(thing.DisplayName ?? "unknown", thing.ReferenceId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? SafeName(Thing thing)
    {
        try
        {
            return thing.DisplayName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? SafeType(Thing thing)
    {
        try
        {
            return thing.PrefabName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static long SafeId(Thing thing)
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
