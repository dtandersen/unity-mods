using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Caev.Tools;

namespace Caev;

/// <summary>
/// A single round-trip to a language model. Implemented by the Unity transport
/// in the plugin and by a scripted fake in tests.
/// </summary>
public interface IAiClient
{
    /// <summary>The model this client talks to, named in answers and logs.</summary>
    string Model { get; }

    Task<AiCompletion> CompleteAsync(
        IReadOnlyList<ChatMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        CancellationToken cancellationToken = default);
}
