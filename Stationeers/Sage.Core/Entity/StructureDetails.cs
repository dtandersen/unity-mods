using System.Collections.Generic;

namespace Sage.Entity;

/// <summary>One readable logic setting: its name, current value, and whether it writes.</summary>
public sealed record StructureLogicValue(string Name, double Value, bool Writable);

/// <summary>One structure slot: its index, slot type, and occupant label.</summary>
public sealed record StructureSlotInfo(int Index, string? SlotType, string Occupant);

/// <summary>The running chip in a housing: its error, display value, and code hash. Null when not a housing or chipless.</summary>
public sealed record ChipState(string? ErrorType, string? ErrorLine, double? Display, string? CodeHash);

/// <summary>One structure on a housing: its d-index, label, and id (0 when the screw is empty).</summary>
public sealed record HousingDeviceEntry(string Index, string? Label, long Id);

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
/// One structure in full: identity, on/off state (null when it has no switch),
/// readable logic settings, slots, and, for housings, the structures on its
/// screws (null for anything else). Storage holds internal reagents for
/// fabricators and furnaces (null when the structure stores none). Build tells
/// how far construction has come (null when the game reports no stages).
/// Live readings are absent when the game does not expose them.
/// </summary>
public sealed record StructureDetails(
    string? Name,
    string? PrefabName,
    long Id,
    IReadOnlyList<NetworkConnection> Connections,
    bool? IsOn,
    IReadOnlyList<StructureLogicValue>? Logic,
    IReadOnlyList<StructureSlotInfo>? Slots,
    IReadOnlyList<HousingDeviceEntry>? Devices,
    ChipState? Chip,
    IReadOnlyList<ReagentAmount>? Storage,
    BuildProgress? Build);
