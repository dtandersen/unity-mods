using System.Collections.Generic;

namespace Caev.Tools;

/// <summary>Arguments supplied to a tool by the model.</summary>
public sealed record ToolArguments(IReadOnlyDictionary<string, string> Values)
{
    public static ToolArguments Empty { get; } =
        new ToolArguments(new Dictionary<string, string>());

    /// <summary>The argument value, or null if it was not supplied.</summary>
    public string Get(string name)
    {
        return Values != null && Values.TryGetValue(name, out var value) ? value : null;
    }
}
