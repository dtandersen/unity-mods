using System;
using System.Collections.Generic;
using System.Linq;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>In-memory structure catalog. Used by tests.</summary>
public sealed class InMemoryStructureCatalog : IStructureCatalog
{
    public List<StructureInfo> Structures { get; } = new List<StructureInfo>();

    public long? LastPowerNetworkId { get; private set; }

    public long? LastDataNetworkId { get; private set; }

    public bool? LastLogic { get; private set; }

    public long? LastRoom { get; private set; }

    public IReadOnlyList<StructureInfo> Search(string? query, long? powerNetworkId, long? dataNetworkId, bool? logic, long? room)
    {
        LastPowerNetworkId = powerNetworkId;
        LastDataNetworkId = dataNetworkId;
        LastLogic = logic;
        LastRoom = room;
        return Structures
            .Where(structure =>
                (!powerNetworkId.HasValue || structure.Connections.Any(c => c.Kind == "power" && c.NetworkId == powerNetworkId.Value)) &&
                (!dataNetworkId.HasValue || structure.Connections.Any(c => c.Kind == "data" && c.NetworkId == dataNetworkId.Value)) &&
                (!logic.HasValue || structure.Logic == logic.Value) &&
                (!room.HasValue || structure.RoomId == room.Value) &&
                (string.IsNullOrWhiteSpace(query) ||
                (structure.Name != null && structure.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (structure.PrefabName != null && structure.PrefabName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)))
            .ToArray();
    }
}
