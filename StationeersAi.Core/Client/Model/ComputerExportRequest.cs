namespace StationeersAi.Client.Model;

/// <summary>Burn one computer's screen text onto one housing's chip.</summary>
public sealed class ComputerExportRequest
{
    [FromRoute][ThingId]
    public long Computer { get; set; }

    [FromBody][ThingId]
    public long? Device { get; set; }
}
