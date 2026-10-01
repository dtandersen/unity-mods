using System.Collections.Generic;

namespace StationeersAi.Entity
{
    /// <summary>One readable logic setting: its name, current value, and whether it writes.</summary>
    public sealed record DeviceLogicValue(string Name, double Value, bool Writable);

    /// <summary>One device slot: its index, slot type, and occupant label.</summary>
    public sealed record DeviceSlotInfo(int Index, string Slot, string Occupant);

    /// <summary>
    /// One device in full: identity, on/off state (null when it has no switch),
    /// readable logic settings, and slots. Live readings are absent when the
    /// game does not expose them.
    /// </summary>
    public sealed record DeviceDetails(
        string Name,
        string Type,
        long Thing,
        long NetworkId,
        bool? IsOn,
        IReadOnlyList<DeviceLogicValue> Logic,
        IReadOnlyList<DeviceSlotInfo> Slots);
}
