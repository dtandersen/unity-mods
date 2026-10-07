namespace StationeersAi.Repository;

/// <summary>The computer a lookup resolved to, or why it resolved to none.</summary>
public sealed record ComputerLookup(IComputer? Computer, string? Error);
