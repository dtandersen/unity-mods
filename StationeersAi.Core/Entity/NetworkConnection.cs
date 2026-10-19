namespace StationeersAi.Entity;

/// <summary>
/// One port on a built thing: its kind ("power", "data", "pipe", or
/// "chute"), the port index within that kind, the network it joins, and,
/// for pipes, which mouth it is ("input", "input2", "output",
/// "output2" — output2 often carries liquids, e.g. the furnace liquid
/// out). A station battery reads e.g. power 0, power 1, data 0; a furnace
/// reads pipe input, pipe output, pipe output2. One wire can carry both
/// power and data when entries share a network id.
/// </summary>
public sealed record NetworkConnection(string Kind, int Index, long NetworkId, string? Port = null);
