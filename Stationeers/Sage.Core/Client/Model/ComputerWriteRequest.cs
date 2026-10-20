namespace Sage.Client.Model;

/// <summary>Stage text on a computer's screen, naming the selected chip by hash.</summary>
public sealed class ComputerWriteRequest
{
    [FromRoute][ThingId]
    public long Computer { get; set; }

    [FromBody]
    public string? SourceCode { get; set; }

    [FromBody]
    public string? Hash { get; set; }
}
