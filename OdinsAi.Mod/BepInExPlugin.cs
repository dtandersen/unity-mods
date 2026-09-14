using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;
using OdinsAi.Ai;
using OdinsAi.Ai.OpenAi;
using OdinsAi.Command;
using OdinsAi.Core;
using OdinsAi.Formatting;
using OdinsAi.Mod.Ai;
using OdinsAi.Query;
using OdinsAi.Repository;
using OdinsAi.Tool;
using UnityEngine;

namespace OdinsAi.Mod
{
    /// <summary>
    /// Odin's Eye BepInEx plugin. The Unity/BepInEx adapter: configuration, the
    /// <c>/ai</c> command, and the composition root. All logic lives in
    /// <c>OdinsAi.Core</c>.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.jotunn.jotunn")]
    public class BepInExPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "davidandersen.odinsai";
        public const string ModName = "Odin's Eye";
        public const string ModVersion = "0.1.0";

        public static BepInExPlugin context;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> showThinking;
        public static ConfigEntry<bool> showToolResults;
        public static ConfigEntry<bool> enableCheats;
        public static ConfigEntry<string> endpoint;
        public static ConfigEntry<string> apiKey;
        public static ConfigEntry<string> model;
        public static ConfigEntry<string> systemPrompt;
        public static ConfigEntry<int> maxIterations;
        public static ConfigEntry<int> timeoutSeconds;
        public static ConfigEntry<int> contextMessages;

        // Chat.Update() keeps the chat window active while m_hideTimer < m_hideDelay,
        // so resetting the timer to zero pops the window open.
        private static readonly FieldInfo ChatHideTimer =
            typeof(Chat).GetField("m_hideTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        private static Conversation conversation;

        private static Conversation Conversation => conversation ??= new Conversation(contextMessages.Value);

        public void Awake()
        {
            context = this;

            modEnabled = Config.Bind("General", "Enabled", true, "Enable Odin's Eye.");
            showThinking = Config.Bind("General", "ShowThinking", false,
                "Post the assistant's tool activity to chat while it works.");
            showToolResults = Config.Bind("General", "ShowToolResults", false,
                "Post each tool's raw output (CSV) to chat.");
            enableCheats = Config.Bind("General", "EnableCheats", false,
                "Allow Odin's Eye to scan the world (dungeons, creatures, resources). Off by default.");

            endpoint = Config.Bind("AI", "Endpoint", "https://api.openai.com/v1/chat/completions",
                "OpenAI-compatible chat completions URL.");
            apiKey = Config.Bind("AI", "ApiKey", "", "API key for the endpoint.");
            model = Config.Bind("AI", "Model", "gpt-4o-mini", "Model name.");
            systemPrompt = Config.Bind("AI", "SystemPrompt",
                "You are Odin's Eye, a Valheim assistant. Use the provided tools to look up game data before answering. Keep answers short and use simple Markdown (**bold** and bullet lists); do not use tables.",
                "System prompt sent to the model.");
            maxIterations = Config.Bind("AI", "MaxToolIterations", 5, "Maximum tool round-trips per question.");
            timeoutSeconds = Config.Bind("AI", "TimeoutSeconds", 120, "HTTP timeout for model requests, in seconds. Raise it if the model is slow with a large conversation.");
            contextMessages = Config.Bind("AI", "ContextMessages", 20, "How many previous messages to send with each question. 0 disables conversation history.");

            if (!modEnabled.Value)
            {
                return;
            }

            CommandManager.Instance.AddConsoleCommand(new AiCommand());

            Dbgl($"Loaded {ModName} v{ModVersion} (core {CoreAssembly.Version})");
        }

        public static void Dbgl(string message)
        {
            Debug.Log($"{typeof(BepInExPlugin).Namespace} {message}");
        }

        /// <summary>
        /// Handles one <c>/ai</c> question. Runs on the Unity main thread up to the
        /// first await, so the game-data snapshot and the HTTP request are created on
        /// the main thread; later continuations only touch plain C# data.
        /// </summary>
        public async Task AskAsync(Terminal terminal, string prompt)
        {
            try
            {
                var assistant = BuildAssistant(terminal);
                if (assistant == null)
                {
                    Post(terminal, "game data is not loaded yet.");
                    return;
                }

                Post(terminal, "thinking...");
                var answer = await assistant.AskAsync(prompt, contextMessages.Value > 0 ? Conversation : null);
                Post(terminal, MarkdownToTmp.ToTmpRichText(answer));
            }
            catch (AiClientException ex)
            {
                Post(terminal, $"error: {ex.Message}");
                Dbgl(ex.ToString());
            }
            catch (Exception ex)
            {
                Post(terminal, $"error: {ex.Message}");
                Dbgl(ex.ToString());
            }
        }

        private static void Post(Terminal terminal, string text)
        {
            terminal.AddString($"{ModName}: {text}");
            OpenChatWindow();
        }

        private static void OpenChatWindow()
        {
            if (Chat.instance != null && ChatHideTimer != null)
            {
                ChatHideTimer.SetValue(Chat.instance, 0f);
            }
        }

        public Task ClearContextAsync(Terminal terminal)
        {
            Conversation.Clear();
            Post(terminal, $"context cleared ({Conversation.Messages.Count} messages).");
            return Task.CompletedTask;
        }

        public async Task CompactContextAsync(Terminal terminal)
        {
            try
            {
                var assistant = BuildAssistant(terminal);
                if (assistant == null)
                {
                    Post(terminal, "game data is not loaded yet.");
                    return;
                }

                if (Conversation.Messages.Count == 0)
                {
                    Post(terminal, "nothing to compact.");
                    return;
                }

                Post(terminal, "compacting context...");
                var summary = await assistant.CompactAsync(Conversation);
                Post(terminal, string.IsNullOrWhiteSpace(summary)
                    ? "compaction produced no summary."
                    : "context compacted.");
            }
            catch (Exception ex)
            {
                Post(terminal, $"error: {ex.Message}");
                Dbgl(ex.ToString());
            }
        }

        private static AiAssistant BuildAssistant(Terminal terminal)
        {
            var database = ObjectDB.instance;
            if (database == null)
            {
                return null;
            }

            IItemRepository items = new ValheimItemRepository(database);
            IRecipeRepository recipes = new ValheimRecipeRepository(database);
            IInventoryRepository inventory = new ValheimInventoryRepository();

            var itemIndex = new ItemIndex(items);

            var toolList = new List<ITool>
            {
                new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(recipes))),
                new SearchFoodTool(new SearchFoodCommand(itemIndex)),
                new SearchItemsTool(new SearchItemsCommand(itemIndex)),
                new SearchInventoryTool(new SearchInventoryCommand(new InventoryIndex(inventory))),
                new SearchStorageTool(new SearchStorageCommand(new StorageIndex(new ValheimStorageRepository()))),
            };

            // World scanning is opt-in: without cheats the model can't see through
            // walls to dungeons, creatures or resource nodes.
            if (enableCheats.Value)
            {
                toolList.Add(new SearchDungeonsTool(new SearchDungeonsCommand(new DungeonIndex(new ValheimDungeonRepository()))));
                toolList.Add(new SearchCreaturesTool(new SearchCreaturesCommand(new CreatureIndex(new ValheimCreatureRepository()))));
                toolList.Add(new SearchResourcesTool(new SearchResourcesCommand(new ResourceIndex(new ValheimResourceRepository()))));
            }

            var tools = new ToolHandler(toolList);

            var client = new OpenAiChatClient(
                new UnityWebRequestTransport(timeoutSeconds.Value),
                new OpenAiOptions(endpoint.Value, apiKey.Value, model.Value));

            Action<string> thinking = showThinking.Value
                ? line => Post(terminal, line)
                : (Action<string>)null;
            Action<string> toolOutput = showToolResults.Value
                ? line => Post(terminal, line)
                : (Action<string>)null;

            return new AiAssistant(client, tools, systemPrompt.Value, maxIterations.Value, Dbgl, thinking, toolOutput);
        }
    }
}