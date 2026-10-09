namespace Caev;

/// <summary>
/// One turn of work for an agent: the question, the history to send with it, the
/// instructions ahead of that history, and how many tool round-trips it may take.
/// The agent reads a turn and keeps nothing from it, so every ask stands alone.
/// </summary>
/// <param name="Prompt">The question, as the stationeer wrote it.</param>
/// <param name="Conversation">History sent with the request. Null sends none.</param>
/// <param name="SystemPrompt">Instructions ahead of the history. Null or blank sends none.</param>
/// <param name="MaxIterations">Tool round-trips allowed before the agent gives up.</param>
public sealed record TurnContext(
    string Prompt,
    Conversation? Conversation,
    string? SystemPrompt,
    int MaxIterations);
