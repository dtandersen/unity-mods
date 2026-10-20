namespace Sage.Client.Model;

/// <summary>One pipe network by id, or pipe networks joined to a device.</summary>
public sealed class PipeNetworkReadRequest
{
    [FromRoute]
    public long? PipeNetwork { get; set; }

    [FromQuery]
    public string? Device { get; set; }
}
