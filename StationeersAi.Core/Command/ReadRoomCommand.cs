using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: read one room's atmospherics.</summary>
public sealed class ReadRoomCommand
{
    private readonly IRoomInspector _rooms;
    private readonly IPlayerContext _player;

    public ReadRoomCommand(IRoomInspector rooms, IPlayerContext player)
    {
        _rooms = rooms;
        _player = player;
    }

    /// <param name="roomId">The room to read. Null reads the room the player stands in.</param>
    /// <returns>The room's atmospherics, or null when it cannot be read.</returns>
    public RoomAtmosphere? Execute(long? roomId)
    {
        var id = roomId ?? _player.Status?.RoomId;
        if (!id.HasValue)
        {
            return null;
        }

        return _rooms.Describe(id.Value);
    }
}
