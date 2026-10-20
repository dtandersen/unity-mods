namespace Sage.Client.Model;

/// <summary>Move one computer's dropdown to one housing on its network.</summary>
public sealed class ComputerSelectRequest
{
    [FromRoute][ThingId]
    public long Computer { get; set; }

    [FromBody][ThingId]
    public long? Device { get; set; }
}
