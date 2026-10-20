using System.Collections.Generic;
using Sage.Entity;
using Sage.Repository;

namespace Sage.Command;

/// <summary>Ask for one pipe network by id, or all pipe networks joined to a device.</summary>
public sealed record ReadPipeNetworkRequest(long? PipeNetworkId, string? Device);

/// <summary>The pipe networks found, or why the mod cannot say.</summary>
public sealed record PipeNetworkResult(Status Status, List<string> Errors, IReadOnlyList<PipeNetworkDetails> PipeNetworks) : BaseResult(Status, Errors);

/// <summary>Domain operation: read pipe networks by id or device.</summary>
public sealed class ReadPipeNetworkCommand : Command<ReadPipeNetworkRequest, PipeNetworkResult>
{
    private readonly IPipeNetworkInspector _pipeNetworks;

    public ReadPipeNetworkCommand(IPipeNetworkInspector pipeNetworks)
    {
        _pipeNetworks = pipeNetworks;
    }

    public PipeNetworkResult Execute(ReadPipeNetworkRequest request)
    {
        if (request.PipeNetworkId.HasValue)
        {
            var one = _pipeNetworks.Describe(request.PipeNetworkId.Value);
            return one == null
                ? new PipeNetworkResult(Status.NOT_FOUND, new List<string> { $"Pipe network {request.PipeNetworkId.Value} was not found." }, new PipeNetworkDetails[0])
                : new PipeNetworkResult(Status.SUCCESS, new List<string>(), new[] { one });
        }

        if (!string.IsNullOrWhiteSpace(request.Device))
        {
            var joined = _pipeNetworks.DescribeDevice(request.Device);
            return joined.Count == 0
                ? new PipeNetworkResult(Status.NOT_FOUND, new List<string> { $"No pipe network joins '{request.Device}'." }, new PipeNetworkDetails[0])
                : new PipeNetworkResult(Status.SUCCESS, new List<string>(), joined);
        }

        return new PipeNetworkResult(Status.BAD_REQUEST, new List<string> { "Pass a pipe network id or a device." }, new PipeNetworkDetails[0]);
    }
}
