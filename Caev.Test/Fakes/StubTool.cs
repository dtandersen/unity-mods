using System;
using System.Collections.Generic;
using Caev.Tools;

namespace Caev.Test.Fakes;

/// <summary>Scripted tool for engine specs: answers from a set reply and records calls.</summary>
public sealed class StubTool : ITool
{
    private readonly List<ToolArguments> _calls = new List<ToolArguments>();
    private string _answer = string.Empty;

    public StubTool(string name)
    {
        Name = name;
        Definition = new ToolDefinition(
            name,
            "Looks things up.",
            new[] { new ToolParameter("query", "What to look up.") });
    }

    public string Name { get; }

    public ToolDefinition Definition { get; }

    public IReadOnlyList<ToolArguments> Calls => _calls;

    public void AnswersWith(string answer)
    {
        _answer = answer ?? string.Empty;
    }

    public ToolResult Execute(ToolArguments arguments)
    {
        _calls.Add(arguments ?? ToolArguments.Empty);
        return new ToolSuccess<string>(_answer, _answer);
    }
}
