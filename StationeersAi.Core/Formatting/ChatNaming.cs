namespace StationeersAi.Formatting;

/// <summary>
/// How the assistant identifies itself in chat. The name is configurable; a
/// blank or whitespace value falls back to <see cref="DefaultName"/>.
/// </summary>
public static class ChatNaming
{
    /// <summary>Name used when the configured value is empty.</summary>
    public const string DefaultName = "Sage";

    /// <summary>The display name for a configured value (trimmed, or the default).</summary>
    public static string Resolve(string? configured)
    {
        return configured is null || string.IsNullOrWhiteSpace(configured) ? DefaultName : configured.Trim();
    }

    /// <summary>A chat line: <c>{name}: {text}</c>.</summary>
    public static string Apply(string? configured, string? text)
    {
        return $"{Resolve(configured)}: {text}";
    }
}
