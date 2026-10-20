using System.Collections.Generic;
using OdinsAi.Tool;

namespace OdinsAi.Ai
{
    /// <summary>
    /// One model response: optional text, requested tool calls, and any reasoning
    /// the provider exposed (e.g. <c>reasoning_content</c>).
    /// </summary>
    public sealed record AiCompletion(
        string Content,
        IReadOnlyList<ToolCall> ToolCalls,
        string Reasoning = null)
    {
        public bool HasToolCalls => ToolCalls != null && ToolCalls.Count > 0;
    }
}