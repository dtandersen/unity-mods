using System;
using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Repository;

/// <summary>In-memory pipe networks. Used by tests.</summary>
public sealed class InMemoryPipeNetworkInspector : IPipeNetworkInspector
{
    private readonly Dictionary<long, PipeNetworkDetails> _pipeNetworks = new Dictionary<long, PipeNetworkDetails>();

    private readonly Dictionary<string, List<long>> _devices = new Dictionary<string, List<long>>(StringComparer.OrdinalIgnoreCase);

    public void Add(PipeNetworkDetails network)
    {
        _pipeNetworks[network.PipeNetworkId] = network;
    }

    public void Join(string device, long pipeNetworkId)
    {
        if (!_devices.TryGetValue(device, out var ids))
        {
            ids = new List<long>();
            _devices[device] = ids;
        }

        if (!ids.Contains(pipeNetworkId))
        {
            ids.Add(pipeNetworkId);
        }
    }

    public PipeNetworkDetails? Describe(long pipeNetworkId)
    {
        PipeNetworkDetails? network;
        return _pipeNetworks.TryGetValue(pipeNetworkId, out network) ? network : null;
    }

    public IReadOnlyList<PipeNetworkDetails> DescribeDevice(string? device)
    {
        if (string.IsNullOrWhiteSpace(device))
        {
            return Array.Empty<PipeNetworkDetails>();
        }

        var key = device!.Trim();
        if (_devices.TryGetValue(key, out var ids))
        {
            var found = new List<PipeNetworkDetails>();
            foreach (var id in ids)
            {
                PipeNetworkDetails? network;
                if (_pipeNetworks.TryGetValue(id, out network) && network != null)
                {
                    found.Add(network);
                }
            }

            return found;
        }

        long idDirect;
        if (long.TryParse(key, out idDirect))
        {
            var direct = Describe(idDirect);
            return direct == null
                ? Array.Empty<PipeNetworkDetails>()
                : new[] { direct };
        }

        return Array.Empty<PipeNetworkDetails>();
    }
}
