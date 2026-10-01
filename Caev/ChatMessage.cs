using System.Collections.Generic;
using Caev.Tools;

namespace Caev;

/// <summary>A message in the conversation sent to the model.</summary>
/// <param name="ToolCalls">Set on assistant messages that request tools.</param>
/// <param name="ToolCallId">Set on tool messages to correlate with a request.</param>
public sealed record ChatMessage(
    ChatRole Role,
    string Content = null,
    IReadOnlyList<ToolCall> ToolCalls = null,
    string ToolCallId = null);
