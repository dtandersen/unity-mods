namespace Sage.Client.Model;

/// <summary>Structure filters: name fragment, power and data networks, logic voice, room.</summary>
public sealed class StructureSearchRequest
{
    [FromQuery("q")]
    public string? Query { get; set; }

    [FromQuery]
    public long? PowerNetworkId { get; set; }

    [FromQuery]
    public long? DataNetworkId { get; set; }

    [FromQuery]
    public bool? Logic { get; set; }

    [FromQuery]
    public long? Room { get; set; }
}
