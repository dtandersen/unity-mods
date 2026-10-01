using System.Collections.Generic;

namespace StationeersAi.Entity;

/// <summary>One readable logic setting: its name, current value, and whether it writes.</summary>
public sealed record DeviceLogicValue(string Name, double Value, bool Writable);

/// <summary>One device slot: its index, slot type, and occupant label.</summary>
public sealed record DeviceSlotInfo(int Index, string Slot, string Occupant);

/// <summary>The running chip in a housing: its error, display value, and code hash. Null when not a housing or chipless.</summary>
public sealed record ChipState(string ErrorType, string ErrorLine, double? Display, string CodeHash);

/// <summary>One device on a housing: its d-index, label, and thing id (0 when the screw is empty).</summary>
public sealed record HousingDeviceEntry(string Index, string Label, long Thing);

/// <summary>
/// One device in full: identity, on/off state (null when it has no switch),
/// readable logic settings, slots, and, for housings, the devices on its
/// screws (null for anything else). Live readings are absent when the game
/// does not expose them.
/// </summary>
public sealed record DeviceDetails(
    string Name,
    string Type,
    long Thing,
    long NetworkId,
    bool? IsOn,
    IReadOnlyList<DeviceLogicValue> Logic,
    IReadOnlyList<DeviceSlotInfo> Slots,
    IReadOnlyList<HousingDeviceEntry> Devices,
    ChipState Chip);
