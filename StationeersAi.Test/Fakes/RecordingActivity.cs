using System.Collections.Generic;
using StationeersAi.Ai;

namespace StationeersAi.Test.Fakes
{
    /// <summary>Records the assistant's intermediate activity for assertions.</summary>
    public sealed class RecordingActivity : IAssistantActivity
    {
        public List<string> Reasoning { get; } = new List<string>();

        public List<string> ToolCalls { get; } = new List<string>();

        public List<string> ToolOutputs { get; } = new List<string>();

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
            ToolOutputs.Add(text);
        }
    }
}
