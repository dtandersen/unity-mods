namespace StationeersAi.Entity
{
    /// <summary>
    /// A built thing: its label, prefab type, thing id, the data network it
    /// sits on (0 when it joins none), and whether it speaks logic.
    /// </summary>
    public sealed record DeviceInfo(string Name, string Type, long Thing, long NetworkId, bool Logic);
}
