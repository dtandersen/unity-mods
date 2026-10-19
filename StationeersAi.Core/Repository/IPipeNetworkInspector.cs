using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>Reads live pipe networks. Null and empty when unknown.</summary>
public interface IPipeNetworkInspector
{
    /// <summary>One pipe network by id, or null when no pipe network matches.</summary>
    PipeNetworkDetails? Describe(long pipeNetworkId);

    /// <summary>All pipe networks joined to a device, or empty when none match.</summary>
    IReadOnlyList<PipeNetworkDetails> DescribeDevice(string? device);
}
