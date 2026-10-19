using System.Collections.Generic;
using Newtonsoft.Json;

namespace StationeersAi.Entity;

/// <summary>
/// One live thing in full: identity, kind, and where it stands, plus the
/// full structure payload when the thing is a built structure on a cable
/// network (null for loose things). Kind is "structure" for built things
/// and "thing" for loose ones. Quantity holds the game text on carried
/// stacks (null when the game gives none).
/// </summary>
public sealed record ThingDetails(
    string? Name,
    string? PrefabName,
    long Id,
    string Kind,
    string? Quantity,
    [property: JsonProperty("position")] StructurePosition Position,
    long RoomId,
    IReadOnlyList<NetworkConnection> Connections,
    bool Logic,
    StructureDetails? Details);
