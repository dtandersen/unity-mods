namespace StationeersAi.Client.Model;

/// <summary>Read one computer's editor screen and dropdown, by thing id.</summary>
public sealed class ComputerReadRequest
{
    [FromRoute][ThingId]
    public long Computer { get; set; }
}
