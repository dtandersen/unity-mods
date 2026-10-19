using System.Collections.Generic;

namespace StationeersAi.Entity;

/// <summary>One pipe network: its air, size, and what sits on it.</summary>
public sealed record PipeNetworkDetails(
    long PipeNetworkId,
    string? ContentType,
    double TemperatureC,
    double PressureKpa,
    double VolumeLitres,
    double TotalMoles,
    IReadOnlyList<GasMoles> Gases,
    IReadOnlyList<string> Members);
