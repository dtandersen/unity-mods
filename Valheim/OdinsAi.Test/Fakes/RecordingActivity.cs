using System.Collections.Generic;
using OdinsAi.Ai;

namespace OdinsAi.Test.Fakes
{
    /// <summary>Captures the activity an assistant reports, for assertions.</summary>
    public sealed class RecordingActivity : IAssistantActivity
    {
        public List<string> Reasoning { get; } = new List<string>();

        public List<string> ToolCalls { get; } = new List<string>();

        public List<string> ToolOutput { get; } = new List<string>();

        public void OnReasoning(string text)
        {
            Reasoning.Add(text);
        }

        public void OnToolCall(string text)
        {
            ToolCalls.Add(text);
        }

        public void OnToolOutput(string text)
        {
            ToolOutput.Add(text);
        }
    }
}