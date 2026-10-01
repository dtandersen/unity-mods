namespace StationeersAi.Entity
{
    /// <summary>
    /// A live logic device: its label, prefab type, reference id, and the data
    /// network it sits on (0 when it joins none).
    /// </summary>
    public sealed record DeviceInfo(string Name, string Type, long ReferenceId, long NetworkId);
}
