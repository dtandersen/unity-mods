using System.Collections.Generic;

namespace StationeersAi.Entity;

/// <summary>One readable logic setting: its name, current value, and whether it writes.</summary>
public sealed record DeviceLogicValue(string Name, double Value, bool Writable);

/// <summary>One device slot: its index, slot type, and occupant label.</summary>
public sealed record DeviceSlotInfo(int Index, string? Slot, string Occupant);

/// <summary>The running chip in a housing: its error, display value, and code hash. Null when not a housing or chipless.</summary>
public sealed record ChipState(string? ErrorType, string? ErrorLine, double? Display, string? CodeHash);

/// <summary>One device on a housing: its d-index, label, and thing id (0 when the screw is empty).</summary>
public sealed record HousingDeviceEntry(string Index, string? Label, long Thing);

/// <summary>One reagent inside a machine: its display name and stored quantity.</summary>
public sealed record ReagentAmount(string Reagent, double Quantity);

/// <summary>
/// One requirement of a build state: what to hold or spend, named the
/// way Stationpedia names it. <see cref="PrefabName"/> is the stable id
/// (e.g. <c>ItemIronSheets</c>), <see cref="Name"/> the display name, and
/// <see cref="Quantity"/> how many the state consumes — null when it is a
/// tool you hold rather than material you spend (e.g. <c>Wrench</c>).
/// </summary>
public sealed record BuildEntry(string PrefabName, string Name, double? Quantity);

/// <summary>
/// One unfinished stage of a placed structure: its 1-based state number and
/// what it requires, in the order the game lists them.
/// </summary>
public sealed record BuildStep(int State, IReadOnlyList<BuildEntry> Entries);

/// <summary>
/// How far construction of a placed structure has come: the current stage
/// (1-based) out of all stages, with one step per unfinished state after
/// it — the game spends the next state's entries, then moves on. The last
/// stage reached means built.
/// </summary>
public sealed record BuildProgress(int Stage, int Stages, IReadOnlyList<BuildStep> BuildStates);

/// <summary>
/// One device in full: identity, on/off state (null when it has no switch),
/// readable logic settings, slots, and, for housings, the devices on its
/// screws (null for anything else). Storage holds internal reagents for
/// fabricators and furnaces (null when the device stores none). Build tells
/// how far construction has come (null when the device is no structure or
/// the game reports no stages). Live readings are absent when the game does
/// not expose them.
/// </summary>
public sealed record DeviceDetails(
    string? Name,
    string? Type,
    long Thing,
    long NetworkId,
    bool? IsOn,
    IReadOnlyList<DeviceLogicValue>? Logic,
    IReadOnlyList<DeviceSlotInfo>? Slots,
    IReadOnlyList<HousingDeviceEntry>? Devices,
    ChipState? Chip,
    IReadOnlyList<ReagentAmount>? Storage,
    BuildProgress? Build);
