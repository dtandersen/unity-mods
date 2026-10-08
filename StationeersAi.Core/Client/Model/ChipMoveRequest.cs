namespace StationeersAi.Client.Model;

/// <summary>One named housing, moved to or from the computer: import, export, or select.</summary>
public sealed class ChipMoveRequest
{
    [FromRoute][ThingId]
    public long Computer { get; set; }

    [FromBody][ThingId]
    public long? Device { get; set; }
}
