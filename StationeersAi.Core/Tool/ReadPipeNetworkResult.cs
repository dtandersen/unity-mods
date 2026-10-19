using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Tool;

/// <summary>The pipe networks a read_pipenetwork call found.</summary>
public sealed record ReadPipeNetworkResult(IReadOnlyList<PipeNetworkDetails> PipeNetworks);
