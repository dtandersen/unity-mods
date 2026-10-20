using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sage.Entity;

/// <summary>Where a built thing stands in meters, serialized lowercase.</summary>
public sealed record StructurePosition(
    [property: JsonProperty("x")] double X,
    [property: JsonProperty("y")] double Y,
    [property: JsonProperty("z")] double Z);

/// <summary>
/// A built thing: its label, prefab name, id, every port on it (power,
/// data, pipe, chute — empty when it joins none; one wire can carry both
/// power and data when entries share a network id), whether it speaks
/// logic, the room it stands in (0 outside rooms), and where it stands.
/// </summary>
public sealed record StructureInfo(string Name, string PrefabName, long Id, IReadOnlyList<NetworkConnection> Connections, bool Logic, long RoomId, [property: JsonProperty("position")] StructurePosition Position);
