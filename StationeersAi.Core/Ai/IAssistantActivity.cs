namespace StationeersAi.Ai
{
    /// <summary>
    /// Receives the assistant's intermediate activity as it happens. The assistant
    /// only reports what happened; the implementor decides whether to show it (and
    /// when to show a placeholder instead).
    /// </summary>
    public interface IAssistantActivity
    {
        /// <summary>The model's reasoning or the narration accompanying a tool call.</summary>
        void OnReasoning(string text);

        /// <summary>A tool the model asked to run, formatted as <c>name(args)</c>.</summary>
        void OnToolCall(string text);

        /// <summary>The raw result a tool returned.</summary>
        void OnToolOutput(string text);
    }
}
