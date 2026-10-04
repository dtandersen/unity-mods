using System;

namespace Caev;

/// <summary>Recognises a chat mention like <c>!sage &lt;question&gt;</c>.</summary>
public static class Mention
{
    /// <summary>
    /// Parses <paramref name="text"/> as a mention of <paramref name="trigger"/>.
    /// The trigger must start the message and be followed by a boundary, so
    /// <c>!sageson</c> is not a mention.
    /// </summary>
    public static bool TryParse(string trigger, string text, out string prompt)
    {
        prompt = string.Empty;

        if (string.IsNullOrWhiteSpace(trigger) || string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.TrimStart();
        if (!trimmed.StartsWith(trigger, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rest = trimmed.Substring(trigger.Length);
        if (rest.Length > 0 && !IsBoundary(rest[0]))
        {
            return false;
        }

        prompt = rest.TrimStart(' ', '\t', ':', ',', '-', '.', '!', '?').Trim();
        return prompt.Length > 0;
    }

    private static bool IsBoundary(char c)
    {
        return char.IsWhiteSpace(c) || c == ':' || c == ',' || c == '-' || c == '.' || c == '!' || c == '?';
    }
}
