namespace OdinsAi.Ai.OpenAi
{
    /// <summary>Connection settings for an OpenAI-compatible chat completions endpoint.</summary>
    /// <param name="ReasoningEffort">
    /// Optional reasoning effort (e.g. <c>low</c>, <c>medium</c>, <c>high</c>). Only
    /// sent when set; valid values are provider/model specific.
    /// </param>
    public sealed record OpenAiOptions(
        string Endpoint,
        string ApiKey,
        string Model,
        string ReasoningEffort = null);
}