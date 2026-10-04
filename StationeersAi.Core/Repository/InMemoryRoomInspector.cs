using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory room inspector. Used by tests.</summary>
public sealed class InMemoryRoomInspector : IRoomInspector
{
    private readonly Dictionary<long, RoomAtmosphere> _rooms = new Dictionary<long, RoomAtmosphere>();

    public void Add(RoomAtmosphere room)
    {
        _rooms[room.RoomId] = room;
    }

    public RoomAtmosphere? Describe(long roomId)
    {
        RoomAtmosphere? room;
        return _rooms.TryGetValue(roomId, out room) ? room : null;
    }
}
