namespace Caev.OpenAi;

/// <summary>Connection settings for an OpenAI-compatible chat completions endpoint.</summary>
/// <param name="ReasoningEffort">
/// Optional reasoning effort (e.g. <c>low</c>, <c>medium</c>, <c>high</c>). May be
/// null, in which case it is not sent; valid values are provider/model specific.
/// </param>
public sealed record OpenAiOptions(
    string Endpoint,
    string ApiKey,
    string Model,
    string ReasoningEffort);
