using System;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;
using OdinsAi.Ai;
using OdinsAi.Ai.OpenAi;
using OdinsAi.Command;
using OdinsAi.Core;
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
        public static ConfigEntry<string> endpoint;
        public static ConfigEntry<string> apiKey;
        public static ConfigEntry<string> model;
        public static ConfigEntry<string> systemPrompt;
        public static ConfigEntry<int> maxIterations;

        public void Awake()
        {
            context = this;

            modEnabled = Config.Bind("General", "Enabled", true, "Enable Odin's Eye.");
            endpoint = Config.Bind("AI", "Endpoint", "https://api.openai.com/v1/chat/completions",
                "OpenAI-compatible chat completions URL.");
            apiKey = Config.Bind("AI", "ApiKey", "", "API key for the endpoint.");
            model = Config.Bind("AI", "Model", "gpt-4o-mini", "Model name.");
            systemPrompt = Config.Bind("AI", "SystemPrompt",
                "You are Odin's Eye, a Valheim assistant. Use the provided tools to look up recipes and food stats before answering.",
                "System prompt sent to the model.");
            maxIterations = Config.Bind("AI", "MaxToolIterations", 5, "Maximum tool round-trips per question.");

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
                var assistant = BuildAssistant();
                if (assistant == null)
                {
                    terminal.AddString($"{ModName}: game data is not loaded yet.");
                    return;
                }

                terminal.AddString($"{ModName}: thinking...");
                var answer = await assistant.AskAsync(prompt);
                terminal.AddString($"{ModName}: {answer}");
            }
            catch (AiClientException ex)
            {
                terminal.AddString($"{ModName} error: {ex.Message}");
                Dbgl(ex.ToString());
            }
            catch (Exception ex)
            {
                terminal.AddString($"{ModName} error: {ex.Message}");
                Dbgl(ex.ToString());
            }
        }

        private static AiAssistant BuildAssistant()
        {
            var database = ObjectDB.instance;
            if (database == null)
            {
                return null;
            }

            IItemRepository items = new ValheimItemRepository(database);
            IRecipeRepository recipes = new ValheimRecipeRepository(database);

            var tools = new ToolHandler(new ITool[]
            {
                new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(recipes))),
                new SearchFoodTool(new SearchFoodCommand(new ItemIndex(items))),
            });

            var client = new OpenAiChatClient(
                new UnityWebRequestTransport(),
                new OpenAiOptions(endpoint.Value, apiKey.Value, model.Value));

            return new AiAssistant(client, tools, systemPrompt.Value, maxIterations.Value);
        }
    }
}