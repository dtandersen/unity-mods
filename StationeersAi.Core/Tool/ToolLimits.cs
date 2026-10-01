namespace StationeersAi.Tool;

/// <summary>Limits on how much tool output is handed back to the model.</summary>
internal static class ToolLimits
{
    /// <summary>Maximum number of entries a search tool returns.</summary>
    public const int MaxResults = 10;

    /// <summary>Maximum number of entries a logic-only device search returns.</summary>
    public const int MaxLogicResults = 30;
}
