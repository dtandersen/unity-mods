namespace StationeersAi.Entity
{
    /// <summary>
    /// A built thing: its label, prefab type, thing id, the data network it
    /// sits on (0 when it joins none), whether it speaks logic, and the room
    /// it stands in (0 outside rooms).
    /// </summary>
    public sealed record DeviceInfo(string Name, string Type, long Thing, long NetworkId, bool Logic, long RoomId);
}
