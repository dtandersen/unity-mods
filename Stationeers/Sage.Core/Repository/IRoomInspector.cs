using Sage.Entity;

namespace Sage.Repository;

/// <summary>Reads a room's atmospherics. Null when the room is unknown.</summary>
public interface IRoomInspector
{
    RoomAtmosphere? Describe(long roomId);
}
