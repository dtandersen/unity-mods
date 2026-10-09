using System;
using System.Threading;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using BepInEx;
using HarmonyLib;
using StationeersAi.Core;
using StationeersAi.Game;
using StationeersAi.Game.Patch;
using StationeersAi.Window;
using Util.Commands;

namespace StationeersAi.Mod;

/// <summary>
/// Sage BepInEx shell. Patches, composes the session objects (assistant
/// factory, view, presenter, mod), and loads/unloads the mod per level.
/// The mod only plugs those objects into the loaded level.
/// </summary>
[BepInPlugin(ModGuid, ModName, ModVersion)]
public class BepInExPlugin : BaseUnityPlugin
{
    public const string ModGuid = "dtandersen.sage";
    public const string ModName = "SAGE";
    public const string ModVersion = "0.1.0";

    // Roots the shell (and through it, the mod) so the scene wipe cannot
    // collect them mid-level. Harmony patches call back through this.
    private static BepInExPlugin? _instance;

    private SageConfig? _settings;
    private MainThreadExecutor? _executor;
    private BepinExLogger? _log;
    private SageMod? _mod;

    public void Awake()
    {
        _instance = this;
        if (Logger == null)
        {
            throw new InvalidOperationException($"{ModName}: BepInEx log source is unavailable.");
        }

        _log = new BepinExLogger(Logger);
        var synchronizationContext = SynchronizationContext.Current;
        var unityThreadId = Thread.CurrentThread.ManagedThreadId;
        _executor = new MainThreadExecutor(synchronizationContext, unityThreadId, _log);
        _log.Info($"captured Unity thread {unityThreadId}; synchronization context available={synchronizationContext != null}");

        _settings = new SageConfig(Config);
        if (!_settings.ModEnabled.Value)
        {
            return;
        }

        var harmony = new Harmony(ModGuid);
        Patch(harmony, typeof(ChatMessagePatch));
        Patch(harmony, typeof(SageOverlayPatch));
        Patch(harmony, typeof(LeaveGamePatch));

        WorldManager.OnWorldStarted += LoadMod;
        PatchHooks.ChatMessage = message => _mod?.HandleChatMessage(message);
        PatchHooks.LeaveGame = () => UnloadMod();

        var commands = new CommandFactory(_executor, _log);
        var agents = new SageAgentFactory(_settings, Paths.ConfigPath, commands, _executor, _log);
        var view = new SwitchingSageView();
        _mod = new SageMod(_settings, agents, view, _executor, _log);
        CommandLine.AddCommand("ai", new AiConsoleCommand(_mod));
        _log.Info("registered 'ai' console command");

        _log.Info($"Loaded {ModName} v{ModVersion} (core {CoreAssembly.Version})");
    }

    private void OnDestroy()
    {
        UnloadMod();
        _log?.Flush();
    }

    /// <summary>Loads the permanent mod for the level. No-op when down.</summary>
    private void LoadMod()
    {
        if (_settings is null || _executor is null || _log is null)
        {
            return;
        }

        _mod?.Load();
    }

    /// <summary>Tears down the mod for the unloading level. No-op when down.</summary>
    private void UnloadMod()
    {
        if (GameManager.GameState != GameState.Running)
        {
            // Prevent unnecessary unload when transitioning from main menu to game
            return;
        }

        try
        {
            _mod?.Unload();
        }
        catch (Exception ex)
        {
            _log?.Error(ex);
        }
        finally
        {
            _log?.Flush();
        }
    }

    private void Patch(Harmony harmony, Type patch)
    {
        try
        {
            harmony.PatchAll(patch);
            Logger.LogInfo($"{ModName}: patched {patch.Name}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"{ModName}: patch {patch.Name} fell through: {ex.Message}");
        }
    }
}
