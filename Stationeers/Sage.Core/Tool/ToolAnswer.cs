using Sage.Command;

namespace Sage.Tool;

/// <summary>One failure line for the model: the command's sentences, or a plain name.</summary>
internal static class ToolAnswer
{
    internal static string Line(BaseResult result, string fallback)
    {
        return result.Errors.Count == 0 ? fallback : string.Join(" ", result.Errors);
    }
}
