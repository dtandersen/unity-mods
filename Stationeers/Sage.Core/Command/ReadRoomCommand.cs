using System.Collections.Generic;
using Sage.Entity;
using Sage.Repository;

namespace Sage.Command;

/// <summary>Ask for one room, or for the room the player stands in when the id is null.</summary>
public sealed record ReadRoomRequest(long? RoomId);

/// <summary>The room's atmospherics, or why the mod cannot say.</summary>
public sealed record RoomResult(Status Status, List<string> Errors, RoomAtmosphere? Room) : BaseResult(Status, Errors);

/// <summary>
/// Domain operation: read one room's air. No id and no room under the
/// player raises <see cref="Status.NOT_FOUND"/> saying which of the two was
/// missing.
/// </summary>
public sealed class ReadRoomCommand : Command<ReadRoomRequest, RoomResult>
{
    private readonly IRoomInspector _rooms;
    private readonly IPlayerContext _player;

    public ReadRoomCommand(IRoomInspector rooms, IPlayerContext player)
    {
        _rooms = rooms;
        _player = player;
    }

    public RoomResult Execute(ReadRoomRequest request)
    {
        var id = request.RoomId ?? _player.Status?.RoomId;
        if (!id.HasValue)
        {
            return new RoomResult(Status.NOT_FOUND, new List<string> { "You are outside a room." }, null);
        }

        var room = _rooms.Describe(id.Value);
        return room == null
            ? new RoomResult(Status.NOT_FOUND, new List<string> { $"Room {id.Value} was not found." }, null)
            : new RoomResult(Status.SUCCESS, new List<string>(), room);
    }
}
