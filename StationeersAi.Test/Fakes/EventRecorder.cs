using System.Collections.Generic;
using Caev;

namespace StationeersAi.Test.Fakes;

/// <summary>Records what an agent turn reported, for assertions.</summary>
public sealed class EventRecorder
{
    public EventRecorder(AgentEvents events)
    {
        events.OnReasoning += Reasoning.Add;
        events.OnToolCall += ToolCalls.Add;
        events.OnToolOutput += ToolOutputs.Add;
        events.OnAnswer += Answers.Add;
    }

    public List<string> Reasoning { get; } = new List<string>();

    public List<string> ToolCalls { get; } = new List<string>();

    public List<string> ToolOutputs { get; } = new List<string>();

    public List<string> Answers { get; } = new List<string>();
}
