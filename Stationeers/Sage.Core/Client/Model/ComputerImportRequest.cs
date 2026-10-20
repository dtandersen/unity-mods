namespace Sage.Client.Model;

/// <summary>Pull the chip under one computer's dropdown into its screen.</summary>
public sealed class ComputerImportRequest
{
    [FromRoute][ThingId]
    public long Computer { get; set; }
}
