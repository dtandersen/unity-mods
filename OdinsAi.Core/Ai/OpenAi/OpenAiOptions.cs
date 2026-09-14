namespace OdinsAi.Ai.OpenAi
{
    /// <summary>Connection settings for an OpenAI-compatible chat completions endpoint.</summary>
    public sealed record OpenAiOptions(string Endpoint, string ApiKey, string Model);
}