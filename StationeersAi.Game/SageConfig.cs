using BepInEx.Configuration;
using StationeersAi.Formatting;
using UnityEngine;

namespace StationeersAi.Game;

/// <summary>
/// All Sage settings. Bound once at startup; entries stay live, so edits
/// apply without a restart. The plugin holds one instance.
/// </summary>
public sealed class SageConfig
{
    public ConfigEntry<bool> ModEnabled { get; }

    public ConfigEntry<bool> ShowThinking { get; }

    public ConfigEntry<bool> ShowToolCalls { get; }

    public ConfigEntry<bool> ShowToolResults { get; }

    public ConfigEntry<bool> RespondToMentions { get; }

    public ConfigEntry<KeyboardShortcut> WindowToggle { get; }

    public ConfigEntry<string> MentionTrigger { get; }

    public ConfigEntry<string> AiName { get; }

    public ConfigEntry<string> Endpoint { get; }

    public ConfigEntry<string> ApiKey { get; }

    public ConfigEntry<string> Model { get; }

    public ConfigEntry<string> ReasoningEffort { get; }

    public ConfigEntry<string> SystemPrompt { get; }

    public ConfigEntry<int> MaxIterations { get; }

    public ConfigEntry<int> TimeoutSeconds { get; }

    public ConfigEntry<int> ContextBytes { get; }

    public ConfigEntry<bool> ServerEnabled { get; }

    public ConfigEntry<int> ServerPort { get; }

    public ConfigEntry<string> ServerToken { get; }

    public SageConfig(ConfigFile config)
    {
        ModEnabled = config.Bind("General", "Enabled", true, "Enable Sage.");
        ShowThinking = config.Bind("General", "ShowThinking", false,
            "Print the model's reasoning and narration to the Sage window while it works.");
        ShowToolCalls = config.Bind("General", "ShowToolCalls", false,
            "Print each tool call (name and arguments) to the Sage window.");
        ShowToolResults = config.Bind("General", "ShowToolResults", false,
            "Print each tool's raw output (CSV) to the Sage window.");
        RespondToMentions = config.Bind("General", "RespondToMentions", true,
            "Answer chat messages that start with the mention trigger (e.g. @sage). In a local game or an empty server every message is answered directly. The answer is posted to chat as the assistant.");
        WindowToggle = config.Bind("General", "WindowToggle", new KeyboardShortcut(KeyCode.F7),
            "Key that opens the Sage window for typing to the assistant directly.");
        MentionTrigger = config.Bind("General", "MentionTrigger", "@sage",
            "Chat prefix that asks the assistant a question.");
        AiName = config.Bind("General", "Name", "Sage",
            "Name the assistant uses. Default: Sage.");

        Endpoint = config.Bind("AI", "Endpoint", "https://api.openai.com/v1/chat/completions",
            "OpenAI-compatible chat completions URL.");
        ApiKey = config.Bind("AI", "ApiKey", "", "API key for the endpoint.");
        Model = config.Bind("AI", "Model", "gpt-4o-mini", "Model name.");
        ReasoningEffort = config.Bind("AI", "ReasoningEffort", "",
            "Optional reasoning effort for reasoning models (e.g. low, medium, high). Leave blank to omit; valid values are model/provider specific.");
        SystemPrompt = config.Bind("AI", "SystemPrompt",
            $"You are {ChatNaming.Resolve(AiName.Value)}, a Stationeers assistant. Use the provided tools to look up game data before answering. Keep answers short and use simple Markdown (**bold**, bullet lists, and small tables). Batch independent searches in one block. Trust conversation history: never re-call a tool whose result is already above — live readings refresh on describe, but static answers (list_instructions, search_structures, search_recipes) never change. Prefer describe_structure without full: Temp and Pressure lead the short list; pass full only for gas ratios. IC facts: lb/sb see only the housing network; sb needs a register, not a bare number; wall heaters need pressure above 6.3 kPa; after Export, the chip hash must equal the editor hash.",
            "System prompt sent to the model.");
        MaxIterations = config.Bind("AI", "MaxToolIterations", 5, "Maximum tool round-trips per question.");
        TimeoutSeconds = config.Bind("AI", "TimeoutSeconds", 120, "HTTP timeout for model requests, in seconds. Raise it if the model is slow with a large conversation.");
        ContextBytes = config.Bind("AI", "ContextBytes", 128000, "How many bytes of conversation history to send with each question. History holds user and assistant text plus tool calls and results, so a byte budget fits few large turns or many small ones. The default is a quarter of a 1M-token window at about 4 bytes per token. 0 disables conversation history.");

        ServerEnabled = config.Bind("Server", "Enabled", false, "Serve Sage game data over HTTP on localhost for outside tools such as Pi.");
        ServerPort = config.Bind("Server", "Port", 8080, "Loopback port for the Sage HTTP server.");
        ServerToken = config.Bind("Server", "Token", "", "Bearer token the HTTP server needs. Leave blank for localhost use with no token.");
    }
}
