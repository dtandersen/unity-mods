namespace StationeersAi.Entity;

/// <summary>A computer's editor text and pointed housing, or why not.</summary>
public sealed record ReadComputerResult(string? Computer, string? Device, string? SourceCode, string? Error);
