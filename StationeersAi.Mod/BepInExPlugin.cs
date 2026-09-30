using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
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
using StationeersAi.Query;
using StationeersAi.Repository;
using StationeersAi.Tool;
using UnityEngine;
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

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> showThinking;
        public static ConfigEntry<bool> showToolCalls;
        public static ConfigEntry<bool> showToolResults;
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

        private static readonly ConcurrentQueue<Action> MainThreadActions = new ConcurrentQueue<Action>();

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

            modEnabled = Config.Bind("General", "Enabled", true, "Enable Sage.");
            showThinking = Config.Bind("General", "ShowThinking", false,
                "Print the model's reasoning and narration to the console while it works.");
            showToolCalls = Config.Bind("General", "ShowToolCalls", false,
                "Print each tool call (name and arguments) to the console.");
            showToolResults = Config.Bind("General", "ShowToolResults", false,
                "Print each tool's raw output (CSV) to the console.");
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

            CommandLine.AddCommand("ai", new AiConsoleCommand());

            Dbgl($"Loaded {ModName} v{ModVersion} (core {CoreAssembly.Version})");
        }

        public static void Dbgl(string message)
        {
            Debug.Log($"{typeof(BepInExPlugin).Namespace} {message}");
        }

        private void Update()
        {
            while (MainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Dbgl(ex.ToString());
                }
            }
        }

        /// <summary>Queues work to run on the Unity main thread.</summary>
        public static void RunOnMainThread(Action action)
        {
            MainThreadActions.Enqueue(action);
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
                Dbgl,
                showThinking: showThinking.Value,
                showToolCalls: showToolCalls.Value,
                showToolResults: showToolResults.Value);
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
            Dbgl($"rebuilt world catalogues ({count} prefabs)");
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
            });

            var client = new OpenAiChatClient(
                new UnityWebRequestTransport(timeoutSeconds.Value),
                new OpenAiOptions(endpoint.Value, apiKey.Value, model.Value, reasoningEffort.Value));

            return new AiAssistant(client, tools, systemPrompt.Value, maxIterations.Value, Dbgl, activity);
        }
    }
}
