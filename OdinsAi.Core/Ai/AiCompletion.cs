using System.Collections.Generic;
using OdinsAi.Tool;

namespace OdinsAi.Ai
{
    /// <summary>One model response: optional text and/or requested tool calls.</summary>
    public sealed record AiCompletion(string Content, IReadOnlyList<ToolCall> ToolCalls)
    {
        public bool HasToolCalls => ToolCalls != null && ToolCalls.Count > 0;
    }
}