namespace StationeersAi.Client.Model;

/// <summary>Device filters: name fragment, network, logic voice, room.</summary>
public sealed class DeviceSearchRequest
{
    [FromQuery("q")]
    public string? Query { get; set; }

    [FromQuery]
    public long? Network { get; set; }

    [FromQuery]
    public bool? Logic { get; set; }

    [FromQuery]
    public long? Room { get; set; }
}
