using Newtonsoft.Json;

namespace StationeersAi.Entity;

/// <summary>Where a built thing stands in meters, serialized lowercase.</summary>
public sealed record DevicePosition(
    [property: JsonProperty("x")] double X,
    [property: JsonProperty("y")] double Y,
    [property: JsonProperty("z")] double Z);

/// <summary>
/// A built thing: its label, prefab name, id, the data network it sits on
/// (0 when it joins none), whether it speaks logic, the room it stands in
/// (0 outside rooms), and where it stands.
/// </summary>
public sealed record DeviceInfo(string Name, string PrefabName, long Id, long NetworkId, bool Logic, long RoomId, [property: JsonProperty("position")] DevicePosition Position);
