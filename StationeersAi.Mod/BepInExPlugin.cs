using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
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

        public static SageWindow window;

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
        public static ConfigEntry<KeyboardShortcut> windowToggle;
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

        private static readonly Dictionary<string, Conversation> mentionConversations = new Dictionary<string, Conversation>();

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
            windowToggle = Config.Bind("General", "WindowToggle", new KeyboardShortcut(KeyCode.F7),
                "Key that opens the Sage window for typing to the assistant directly.");
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
                $"You are {AssistantName}, a Stationeers assistant. Use the provided tools to look up game data before answering. Keep answers short and use simple Markdown (**bold**, bullet lists, and small tables). Batch independent searches in one block. Trust conversation history: never re-call a tool whose result is already above — live readings refresh on describe, but static answers (mips_reference, search_structures, search_recipes) never change. Prefer describe_device without full: Temp and Pressure lead the short list; pass full only for gas ratios. IC facts: lb/sb see only the housing network; sb needs a register, not a bare number; wall heaters need pressure above 6.3 kPa; after Export, the chip hash must equal the editor hash.",
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

            EnsureWindow();
            Log.Log($"Sage window ready ({windowToggle.Value})");

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
        /// Runs work on Unity's main thread and returns its result, blocking the
        /// caller. Tools run off the main thread, but Unity objects demand it.
        /// Throws when the context was never captured or the wait times out.
        /// </summary>
        internal static T RunOnMainThreadSync<T>(Func<T> func, int timeoutMilliseconds = 10000)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            if (Thread.CurrentThread.ManagedThreadId == unityThreadId)
            {
                return func();
            }

            var synchronizationContext = unitySynchronizationContext;
            if (synchronizationContext == null)
            {
                throw new InvalidOperationException("Unity synchronization context was not captured.");
            }

            T result = default(T);
            Exception error = null;
            using (var done = new ManualResetEventSlim(false))
            {
                synchronizationContext.Post(_ =>
                {
                    try
                    {
                        result = func();
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        done.Set();
                    }
                }, null);

                if (!done.Wait(timeoutMilliseconds))
                {
                    throw new TimeoutException("Timed out waiting for Unity's main thread.");
                }
            }

            if (error != null)
            {
                throw error;
            }

            return result;
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

            // Skip our own answers echoing back through the server relay.
            if (message.DisplayName != null &&
                message.DisplayName.Equals(AssistantName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Log.Log($"chat seen from '{message.DisplayName}': {Truncate(message.ChatText, 80)}");

            if (!Mention.TryParse(mentionTrigger.Value, message.ChatText, out var prompt))
            {
                return;
            }

            Log.Log($"mention matched, asking: {Truncate(prompt, 80)}");
            _ = MentionResponderInstance().HandleAsync(prompt, MentionConversation(message.DisplayName));
        }

        /// <summary>One chat session per player, capped like the console history.</summary>
        private static Conversation MentionConversation(string player)
        {
            if (contextMessages.Value <= 0)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(player))
            {
                player = "player";
            }

            if (!mentionConversations.TryGetValue(player, out var conversation))
            {
                conversation = new Conversation(contextMessages.Value);
                mentionConversations[player] = conversation;
            }

            return conversation;
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
        /// The window lives on its own persistent object: scene loads destroy
        /// the plugin's GameObject, which would silently kill Update/OnGUI.
        /// </summary>
        private static void EnsureWindow()
        {
            if (window != null)
            {
                return;
            }

            var holder = new GameObject("SageWindow");
            UnityEngine.Object.DontDestroyOnLoad(holder);
            window = holder.AddComponent<SageWindow>();
        }

        /// <summary>Opens the Sage window. Used by the <c>ai window</c> command.</summary>
        public void ShowWindow()
        {
            EnsureWindow();
            if (window != null)
            {
                Log.Log("Sage window shown via command");
                window.Show();
            }
            else
            {
                Log.Log("Sage window missing on show command");
            }
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

        /// <summary>
        /// Handles one question typed in the Sage window. Answers stream back
        /// into the window through <see cref="SageWindow"/>.
        /// </summary>
        public void AskInWindow(SageWindow window, string prompt)
        {
            if (window == null)
            {
                return;
            }

            window.Busy = true;
            _ = CreateWindowPresenter(window)
                .AskAsync(prompt, contextMessages.Value > 0 ? Conversation : null)
                .ContinueWith(_ => window.Busy = false, TaskScheduler.Default);
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

        private static ChatPresenter CreateWindowPresenter(SageWindow window)
        {
            return new ChatPresenter(
                new WindowChatSink(window.AddLine),
                activity => BuildAssistant(activity),
                PlayerName,
                AssistantName,
                Log,
                showThinking: showThinking.Value,
                showToolCalls: showToolCalls.Value,
                showToolResults: showToolResults.Value,
                clock: StationeersAi.Time.SystemClock.Instance);
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
                new WriteIcTool(new WriteIcCommand(new StationeersIcEditor())),
                new ReadIcTool(new ReadIcCommand(new StationeersIcEditor())),
                new ExportIcTool(new ExportIcCommand(new StationeersIcExport())),
                new MipsReferenceTool(new ReadMipsCommand(new StationeersMipsReference())),
                new SearchDevicesTool(new SearchDevicesCommand(new StationeersDeviceCatalog())),
                new DescribeDeviceTool(new DescribeDeviceCommand(new StationeersDeviceInspector(), new StationeersDeviceCatalog())),
            });

            var client = new OpenAiChatClient(
                new UnityWebRequestTransport(timeoutSeconds.Value),
                new OpenAiOptions(endpoint.Value, apiKey.Value, model.Value, reasoningEffort.Value));

            return new AiAssistant(client, tools, systemPrompt.Value, maxIterations.Value, Log, activity);
        }
    }
}
