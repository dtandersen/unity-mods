namespace StationeersAi.Client.Model;

/// <summary>Thing filters: name fragment and radius.</summary>
public sealed class ThingSearchRequest
{
    [FromQuery("q")]
    public string? Query { get; set; }

    [FromQuery]
    public double? MaxDistance { get; set; }
}
