using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using OdinsAi.Ai;
using OdinsAi.Ai.OpenAi;
using OdinsAi.Command;
using OdinsAi.Core;
using OdinsAi.Formatting;
using OdinsAi.Mod.Ai;
using OdinsAi.Mod.Patch;
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
        public static ConfigEntry<bool> showToolCalls;
        public static ConfigEntry<bool> showToolResults;
        public static ConfigEntry<bool> enableCheats;
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

        private static readonly ConcurrentQueue<Action> MainThreadActions = new ConcurrentQueue<Action>();
        private static MentionResponder mentionResponder;
        private static ObjectDB catalogueDatabase;
        private static IItemRepository catalogueItems;
        private static IRecipeRepository catalogueRecipes;
        private static IBossRepository catalogueBosses;

        // Server-mode chat relay parsing.
        private static readonly int SayMethodHash = "Say".GetStableHashCode();
        private static readonly int ChatMessageMethodHash = "ChatMessage".GetStableHashCode();
        private static readonly ParameterInfo[] SayParameters =
            typeof(Talker).GetMethod("RPC_Say", BindingFlags.Instance | BindingFlags.NonPublic)?.GetParameters();
        private static readonly ParameterInfo[] ChatMessageParameters =
            typeof(Chat).GetMethod("RPC_ChatMessage", BindingFlags.Instance | BindingFlags.NonPublic)?.GetParameters();
        private static bool serverBusy;

        private static Conversation Conversation => conversation ??= new Conversation(contextMessages.Value);

        public void Awake()
        {
            context = this;

            modEnabled = Config.Bind("General", "Enabled", true, "Enable Odin's Eye.");
            showThinking = Config.Bind("General", "ShowThinking", false,
                "Post the model's reasoning and narration to chat while it works.");
            showToolCalls = Config.Bind("General", "ShowToolCalls", false,
                "Post each tool call (name and arguments) to chat.");
            showToolResults = Config.Bind("General", "ShowToolResults", false,
                "Post each tool's raw output (CSV) to chat.");
            enableCheats = Config.Bind("General", "EnableCheats", false,
                "Allow Odin's Eye to scan the world (dungeons, creatures, resources). Off by default.");
            respondToMentions = Config.Bind("General", "RespondToMentions", false,
                "Answer chat mentions in shared chat. Enable on the machine that should act as Odin.");
            mentionTrigger = Config.Bind("General", "MentionTrigger", "!odin",
                "Chat prefix that asks the assistant a question.");
            aiName = Config.Bind("General", "Name", "Odin",
                "Name the assistant uses in chat. Default: Odin.");

            endpoint = Config.Bind("AI", "Endpoint", "https://api.openai.com/v1/chat/completions",
                "OpenAI-compatible chat completions URL.");
            apiKey = Config.Bind("AI", "ApiKey", "", "API key for the endpoint.");
            model = Config.Bind("AI", "Model", "gpt-4o-mini", "Model name.");
            reasoningEffort = Config.Bind("AI", "ReasoningEffort", "",
                "Optional reasoning effort for reasoning models (e.g. low, medium, high). Leave blank to omit; valid values are model/provider specific.");
            systemPrompt = Config.Bind("AI", "SystemPrompt",
                $"You are {AssistantName}, a Valheim assistant. Use the provided tools to look up game data before answering. Keep answers short and use simple Markdown (**bold**, bullet lists, and small tables).",
                "System prompt sent to the model.");
            maxIterations = Config.Bind("AI", "MaxToolIterations", 5, "Maximum tool round-trips per question.");
            timeoutSeconds = Config.Bind("AI", "TimeoutSeconds", 120, "HTTP timeout for model requests, in seconds. Raise it if the model is slow with a large conversation.");
            contextMessages = Config.Bind("AI", "ContextMessages", 20, "How many previous messages to send with each question. 0 disables conversation history.");

            // Chat patches: client mention detection and the server relay watcher.
            var harmony = new Harmony(ModGuid);
            harmony.PatchAll(typeof(ChatMentionPatch));
            harmony.PatchAll(typeof(ServerChatRelayPatch));

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
        /// Called for every chat message. If it is a mention and mention handling is
        /// enabled, asks Odin in shared chat.
        /// </summary>
        public static void OnPlayerChat(string text)
        {
            if (!modEnabled.Value || !respondToMentions.Value)
            {
                return;
            }

            if (!Mention.TryParse(mentionTrigger.Value, text, out var prompt))
            {
                return;
            }

            _ = MentionResponderInstance().HandleAsync(prompt);
        }

        private static MentionResponder MentionResponderInstance()
        {
            return mentionResponder ??= new MentionResponder(
                new BroadcastChatSink(RunOnMainThread),
                () => BuildAssistant(Player.m_localPlayer, null),
                AssistantName,
                Dbgl);
        }

        /// <summary>
        /// Called for every RPC the local peer routes. On a dedicated server this
        /// includes player chat, which the server only relays between clients.
        /// </summary>
        public static void OnServerRoutedRpc(ZRoutedRpc.RoutedRPCData data)
        {
            if (data == null || !modEnabled.Value || !respondToMentions.Value)
            {
                return;
            }

            if (ZNet.instance == null || !ZNet.instance.IsServer() || SayParameters == null || ChatMessageParameters == null)
            {
                return;
            }

            try
            {
                if (data.m_methodHash == SayMethodHash)
                {
                    OnServerSay(data);
                }
                else if (data.m_methodHash == ChatMessageMethodHash)
                {
                    OnServerChatMessage(data);
                }
            }
            catch (Exception ex)
            {
                Dbgl($"chat relay parse failed: {ex}");
            }
        }

        private static void OnServerSay(ZRoutedRpc.RoutedRPCData data)
        {
            // Args for "Say": [sender, int type, UserInfo user, string text]
            var args = ZNetView.Deserialize(data.m_senderPeerID, SayParameters, data.m_parameters);
            if (args.Length < 4 || !(args[2] is UserInfo user))
            {
                return;
            }

            var player = PlayerForZdo(data.m_targetZDO) ?? PlayerForPeer(data.m_senderPeerID);
            DispatchServerMention(player, user, args[3] as string, player != null ? player.GetHeadPoint() : Vector3.zero);
        }

        private static void OnServerChatMessage(ZRoutedRpc.RoutedRPCData data)
        {
            // Args for "ChatMessage": [sender, Vector3 position, int type, UserInfo, string text]
            var args = ZNetView.Deserialize(data.m_senderPeerID, ChatMessageParameters, data.m_parameters);
            if (args.Length < 5 || !(args[3] is UserInfo user))
            {
                return;
            }

            DispatchServerMention(PlayerForPeer(data.m_senderPeerID), user, args[4] as string, (Vector3)args[1]);
        }

        private static void DispatchServerMention(Player player, UserInfo user, string text, Vector3 position)
        {
            if (player == null || serverBusy || !Mention.TryParse(mentionTrigger.Value, text, out var prompt))
            {
                return;
            }

            serverBusy = true;
            Dbgl($"server mention from '{player.GetPlayerName()}': {prompt}");
            var responder = new MentionResponder(
                new ServerChatSink(user, position, RunOnMainThread),
                () => BuildAssistant(player, null),
                AssistantName,
                Dbgl);

            _ = responder.HandleAsync(prompt).ContinueWith(_ => serverBusy = false);
        }

        private static Player PlayerForZdo(ZDOID zdo)
        {
            if (zdo.IsNone() || ZDOMan.instance == null || ZNetScene.instance == null)
            {
                return null;
            }

            var data = ZDOMan.instance.GetZDO(zdo);
            var view = data != null ? ZNetScene.instance.FindInstance(data) : null;
            return view != null ? view.gameObject.GetComponent<Player>() : null;
        }

        private static Player PlayerForPeer(long peerId)
        {
            var peer = ZNet.instance != null ? ZNet.instance.GetPeer(peerId) : null;
            return peer != null ? PlayerForZdo(peer.m_characterID) : null;
        }

        /// <summary>
        /// Handles one <c>/ai</c> question. Runs on the Unity main thread up to the
        /// first await, so the game-data snapshot and the HTTP request are created on
        /// the main thread; later continuations only touch plain C# data.
        /// </summary>
        public Task AskAsync(Terminal terminal, string prompt)
        {
            return CreatePresenter(terminal, Player.m_localPlayer)
                .AskAsync(prompt, contextMessages.Value > 0 ? Conversation : null);
        }

        private static ChatPresenter CreatePresenter(Terminal terminal, Player player)
        {
            return new ChatPresenter(
                new TerminalChatSink(terminal),
                activity => BuildAssistant(player, activity),
                () => PlayerName(player),
                AssistantName,
                Dbgl,
                showThinking: showThinking.Value,
                showToolCalls: showToolCalls.Value,
                showToolResults: showToolResults.Value);
        }

        /// <summary>
        /// Display name for the assistant's chat messages. Configurable via the
        /// <c>General/Name</c> entry; falls back to "Odin" when blank.
        /// </summary>
        public static string AssistantName => ChatNaming.Resolve(aiName?.Value);

        private static string PlayerName(Player player)
        {
            return player != null ? player.GetPlayerName() : null;
        }

        public Task ClearContextAsync(Terminal terminal)
        {
            CreatePresenter(terminal, Player.m_localPlayer).Clear(Conversation);
            return Task.CompletedTask;
        }

        public Task CompactContextAsync(Terminal terminal)
        {
            return CreatePresenter(terminal, Player.m_localPlayer).CompactAsync(Conversation);
        }

        /// <summary>
        /// World data (items, recipes, bosses) does not depend on the player, so it is
        /// built once and reused until the ObjectDB instance changes.
        /// </summary>
        private static void EnsureCatalogues(ObjectDB database)
        {
            if (catalogueItems != null && ReferenceEquals(catalogueDatabase, database))
            {
                return;
            }

            catalogueItems = new ValheimItemRepository(database);
            catalogueRecipes = new ValheimRecipeRepository(database);
            catalogueBosses = new ValheimBossRepository();
            catalogueDatabase = database;
            Dbgl("rebuilt world catalogues");
        }

        private static AiAssistant BuildAssistant(Player player, IAssistantActivity activity)
        {
            var database = ObjectDB.instance;
            if (database == null)
            {
                return null;
            }

            EnsureCatalogues(database);

            var playerContext = new ValheimPlayerContext(database, player, enableCheats.Value);
            var itemIndex = new ItemIndex(catalogueItems);
            var recipeIndex = new RecipeIndex(catalogueRecipes);
            var inventoryIndex = new InventoryIndex(playerContext);
            var storageIndex = new StorageIndex(playerContext);
            var compendium = new CompendiumIndex(playerContext);

            var toolList = new List<ITool>
            {
                new SearchRecipesTool(new SearchRecipesCommand(recipeIndex, playerContext)),
                new SearchUpgradesTool(new SearchUpgradesCommand(recipeIndex, playerContext)),
                new SearchFoodTool(new SearchFoodCommand(itemIndex)),
                new SearchItemsTool(new SearchItemsCommand(itemIndex)),
                new SearchInventoryTool(new SearchInventoryCommand(inventoryIndex)),
                new SearchStorageTool(new SearchStorageCommand(storageIndex)),
                new SearchCompendiumTool(new SearchCompendiumCommand(compendium)),
                new ReadCompendiumTool(new ReadCompendiumCommand(compendium)),
                new GetLocationTool(new GetLocationCommand(playerContext)),
                new ListBossesTool(new GetBossStatusCommand(catalogueBosses)),
            };

            // World scanning is opt-in: without cheats the model can't see through
            // walls to dungeons, creatures or resource nodes.
            if (enableCheats.Value)
            {
                toolList.Add(new SearchDungeonsTool(new SearchDungeonsCommand(new DungeonIndex(new ValheimDungeonRepository(player)))));
                toolList.Add(new SearchCreaturesTool(new SearchCreaturesCommand(new CreatureIndex(new ValheimCreatureRepository(player)))));
                toolList.Add(new SearchResourcesTool(new SearchResourcesCommand(new ResourceIndex(new ValheimResourceRepository(player)))));
            }

            var tools = new ToolHandler(toolList);

            var client = new OpenAiChatClient(
                new UnityWebRequestTransport(timeoutSeconds.Value),
                new OpenAiOptions(endpoint.Value, apiKey.Value, model.Value, reasoningEffort.Value));

            return new AiAssistant(client, tools, systemPrompt.Value, maxIterations.Value, Dbgl, activity);
        }
    }
}