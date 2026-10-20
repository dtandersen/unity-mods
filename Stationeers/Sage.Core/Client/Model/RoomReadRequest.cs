namespace Sage.Client.Model;

/// <summary>One room, or the room the player stands in when no id is given.</summary>
public sealed class RoomReadRequest
{
    [FromRoute]
    public long? Id { get; set; }
}
