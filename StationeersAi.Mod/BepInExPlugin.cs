using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using StationeersAi.Ai;
using StationeersAi.Ai.OpenAi;
using StationeersAi.Command;
using StationeersAi.Core;
using StationeersAi.Formatting;
using StationeersAi.Mod.Ai;
using StationeersAi.Mod.Patch;
using StationeersAi.Query;
using StationeersAi.Repository;
using StationeersAi.Time;
using StationeersAi.Tool;
using Util.Commands;

namespace StationeersAi.Mod
{
    /// <summary>
    /// Sage BepInEx plugin. The Unity/BepInEx adapter: configuration, the
    /// <c>ai</c> console command, and the composition root. All logic lives in
    /// <c>StationeersAi.Core</c>.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class BepInExPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "davidandersen.stationeersai";
        public const string ModName = "Sage";
        public const string ModVersion = "0.1.0";

        public static BepInExPlugin context;

        /// <summary>
        /// The core <see cref="Core.Logger"/> handed to core objects for their
        /// trace. Named <c>Log</c> (not <c>Logger</c>) to avoid hiding
        /// <c>BaseUnityPlugin.Logger</c>. Assigned in <see cref="Awake"/> and
        /// never null afterwards.
        /// </summary>
        public static BepinExLogger Log;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> showThinking;
        public static ConfigEntry<bool> showToolCalls;
        public static ConfigEntry<bool> showToolResults;
        public static ConfigEntry<bool> respondToMentions;
        public static ConfigEntry<string> mentionTrigger;
        public static ConfigEntry<string> aiName;
        public static ConfigEntry<string> endpoint;
        public static ConfigEntry<string> apiKey;
        public static ConfigEntry<string> model;
        public static ConfigEntry<string> reasoningEffort;
        public static ConfigEntry<string> systemPrompt;
        public static ConfigEntry<int> maxIterations;
        public static ConfigEntry<int> timeoutSeconds;
        public static ConfigEntry<int> contextMessages;

        private static Conversation conversation;
        private static SynchronizationContext unitySynchronizationContext;
        private static int unityThreadId;

        private static MentionResponder mentionResponder;

        // World data does not depend on the player, so it is built once and
        // reused until the prefab catalogue changes size.
        private static IItemRepository catalogueItems;
        private static IStructureRepository catalogueStructures;
        private static IRecipeRepository catalogueRecipes;
        private static IStationpediaRepository catalogueStationpedia;
        private static int cataloguePrefabCount = -1;

        private static Conversation Conversation => conversation ??= new Conversation(contextMessages.Value);

        public void Awake()
        {
            context = this;
            if (Logger == null)
            {
                throw new InvalidOperationException($"{ModName}: BepInEx log source is unavailable.");
            }

            Log = new BepinExLogger(Logger);
            unitySynchronizationContext = SynchronizationContext.Current;
            unityThreadId = Thread.CurrentThread.ManagedThreadId;
            Log.Log($"captured Unity thread {unityThreadId}; synchronization context available={unitySynchronizationContext != null}");

            modEnabled = Config.Bind("General", "Enabled", true, "Enable Sage.");
            showThinking = Config.Bind("General", "ShowThinking", false,
                "Print the model's reasoning and narration to the console while it works.");
            showToolCalls = Config.Bind("General", "ShowToolCalls", false,
                "Print each tool call (name and arguments) to the console.");
            showToolResults = Config.Bind("General", "ShowToolResults", false,
                "Print each tool's raw output (CSV) to the console.");
            respondToMentions = Config.Bind("General", "RespondToMentions", true,
                "Answer chat messages that start with the mention trigger (e.g. @sage). The answer is posted to chat as the assistant.");
            mentionTrigger = Config.Bind("General", "MentionTrigger", "@sage",
                "Chat prefix that asks the assistant a question.");
            aiName = Config.Bind("General", "Name", "Sage",
                "Name the assistant uses. Default: Sage.");

            endpoint = Config.Bind("AI", "Endpoint", "https://api.openai.com/v1/chat/completions",
                "OpenAI-compatible chat completions URL.");
            apiKey = Config.Bind("AI", "ApiKey", "", "API key for the endpoint.");
            model = Config.Bind("AI", "Model", "gpt-4o-mini", "Model name.");
            reasoningEffort = Config.Bind("AI", "ReasoningEffort", "",
                "Optional reasoning effort for reasoning models (e.g. low, medium, high). Leave blank to omit; valid values are model/provider specific.");
            systemPrompt = Config.Bind("AI", "SystemPrompt",
                $"You are {AssistantName}, a Stationeers assistant. Use the provided tools to look up game data before answering. Keep answers short and use simple Markdown (**bold**, bullet lists, and small tables).",
                "System prompt sent to the model.");
            maxIterations = Config.Bind("AI", "MaxToolIterations", 5, "Maximum tool round-trips per question.");
            timeoutSeconds = Config.Bind("AI", "TimeoutSeconds", 120, "HTTP timeout for model requests, in seconds. Raise it if the model is slow with a large conversation.");
            contextMessages = Config.Bind("AI", "ContextMessages", 20, "How many previous messages to send with each question. 0 disables conversation history.");

            if (!modEnabled.Value)
            {
                return;
            }

            var harmony = new Harmony(ModGuid);
            harmony.PatchAll(typeof(ChatMessagePatch));
            Log.Log($"patched {harmony.GetPatchedMethods().Count()} method(s) for chat mentions");

            CommandLine.AddCommand("ai", new AiConsoleCommand());
            Log.Log("registered 'ai' console command");

            Log.Log($"Loaded {ModName} v{ModVersion} (core {CoreAssembly.Version})");
        }

        /// <summary>Runs work on Unity's main thread, posting from async continuations.</summary>
        public static void RunOnMainThread(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (Thread.CurrentThread.ManagedThreadId == unityThreadId)
            {
                RunGuarded(action);
                return;
            }

            var synchronizationContext = unitySynchronizationContext;
            if (synchronizationContext == null)
            {
                Log.Log("cannot dispatch to Unity main thread: synchronization context was not captured");
                return;
            }

            synchronizationContext.Post(_ => RunGuarded(action), null);
        }

        private static void RunGuarded(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.Log(ex.ToString());
            }
        }

        /// <summary>
        /// Called for every chat message displayed on this machine. If it is a
        /// mention and mention handling is enabled, asks Sage in chat. Runs on
        /// the Unity main thread, so the game-data snapshot is safe to build.
        /// </summary>
        public static void OnChatMessage(Assets.Scripts.Networking.ChatMessage message)
        {
            if (!modEnabled.Value || !respondToMentions.Value || message == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message.ChatText))
            {
                return;
            }

            Log.Log($"chat seen from '{message.DisplayName}': {Truncate(message.ChatText, 80)}");

            // Skip our own answers echoing back through the server relay.
            if (message.DisplayName != null &&
                message.DisplayName.Equals(AssistantName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!Mention.TryParse(mentionTrigger.Value, message.ChatText, out var prompt))
            {
                return;
            }

            Log.Log($"mention matched, asking: {Truncate(prompt, 80)}");
            _ = MentionResponderInstance().HandleAsync(prompt);
        }

        internal static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
            {
                return value;
            }

            return value.Substring(0, max) + "...";
        }

        private static MentionResponder MentionResponderInstance()
        {
            return mentionResponder ??= new MentionResponder(
                new ChatMessageSink(RunOnMainThread),
                () => BuildAssistant(null),
                AssistantName,
                Log);
        }

        /// <summary>
        /// Handles one <c>ai</c> question. Runs on the Unity main thread up to the
        /// first await, so the game-data snapshot and the HTTP request are created on
        /// the main thread; later continuations only touch plain C# data, and the
        /// console sink marshals its output back to the main thread.
        /// </summary>
        public void Ask(string prompt)
        {
            _ = CreatePresenter()
                .AskAsync(prompt, contextMessages.Value > 0 ? Conversation : null);
        }

        public void ClearContext()
        {
            CreatePresenter().Clear(Conversation);
        }

        public void CompactContext()
        {
            _ = CreatePresenter().CompactAsync(Conversation);
        }

        private static ChatPresenter CreatePresenter()
        {
            return new ChatPresenter(
                new ConsoleChatSink(RunOnMainThread),
                activity => BuildAssistant(activity),
                PlayerName,
                AssistantName,
                Log,
                showThinking: showThinking.Value,
                showToolCalls: showToolCalls.Value,
                showToolResults: showToolResults.Value,
                clock: SystemClock.Instance);
        }

        /// <summary>
        /// Display name for the assistant. Configurable via the
        /// <c>General/Name</c> entry; falls back to "Sage" when blank.
        /// </summary>
        public static string AssistantName => ChatNaming.Resolve(aiName?.Value);

        private static string PlayerName()
        {
            try
            {
                var human = Human.LocalHuman;
                if (human != null && !string.IsNullOrWhiteSpace(human.CustomName))
                {
                    return human.CustomName;
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        /// <summary>
        /// World data (items, structures, recipes, Stationpedia) does not depend on
        /// the player, so it is built once and reused until the prefab catalogue
        /// changes (game load, mods adding prefabs).
        /// </summary>
        private static void EnsureCatalogues()
        {
            var prefabs = Prefab.AllPrefabs;
            var count = prefabs != null ? prefabs.Count : 0;
            if (catalogueItems != null && cataloguePrefabCount == count && count > 0)
            {
                return;
            }

            catalogueItems = new StationeersItemRepository(prefabs);
            catalogueStructures = new StationeersStructureRepository(prefabs);
            catalogueRecipes = new StationeersRecipeRepository();
            catalogueStationpedia = new StationeersStationpediaRepository(Stationpedia.StationpediaPages);
            cataloguePrefabCount = count;
            Log.Log($"rebuilt world catalogues ({count} prefabs)");
        }

        private static AiAssistant BuildAssistant(IAssistantActivity activity)
        {
            var prefabs = Prefab.AllPrefabs;
            if (prefabs == null || prefabs.Count == 0)
            {
                return null;
            }

            EnsureCatalogues();

            Human human = null;
            try
            {
                human = Human.LocalHuman;
            }
            catch (Exception)
            {
            }

            var playerContext = new StationeersPlayerContext(human);

            var tools = new ToolHandler(new ITool[]
            {
                new SearchItemsTool(new SearchItemsCommand(new ItemIndex(catalogueItems))),
                new SearchStructuresTool(new SearchStructuresCommand(new StructureIndex(catalogueStructures))),
                new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(catalogueRecipes))),
                new SearchStationpediaTool(new SearchStationpediaCommand(new StationpediaIndex(catalogueStationpedia))),
                new ReadStationpediaTool(new ReadStationpediaCommand(catalogueStationpedia)),
                new SearchInventoryTool(new SearchInventoryCommand(new InventoryIndex(playerContext))),
                new GetStatusTool(new GetStatusCommand(playerContext)),
                new GetWorldTool(new GetWorldCommand(new StationeersWorldContext())),
            });

            var client = new OpenAiChatClient(
                new UnityWebRequestTransport(timeoutSeconds.Value),
                new OpenAiOptions(endpoint.Value, apiKey.Value, model.Value, reasoningEffort.Value));

            return new AiAssistant(client, tools, systemPrompt.Value, maxIterations.Value, Log, activity);
        }
    }
}
