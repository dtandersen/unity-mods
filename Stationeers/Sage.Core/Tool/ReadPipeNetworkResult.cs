using System.Collections.Generic;
using Sage.Entity;

namespace Sage.Tool;

/// <summary>The pipe networks a read_pipenetwork call found.</summary>
public sealed record ReadPipeNetworkResult(IReadOnlyList<PipeNetworkDetails> PipeNetworks);
