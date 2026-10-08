namespace StationeersAi.Client.Model;

/// <summary>Read the program burned on one housing's chip, by thing id.</summary>
public sealed class CircuitHousingReadRequest
{
    [FromRoute][ThingId]
    public long Device { get; set; }
}
