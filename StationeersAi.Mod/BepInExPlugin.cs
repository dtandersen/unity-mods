using System;
using System.Threading;
using Assets.Scripts;
using BepInEx;
using HarmonyLib;
using StationeersAi.Core;
using StationeersAi.Game;
using StationeersAi.Game.Patch;
using Util.Commands;

namespace StationeersAi.Mod;

/// <summary>
/// Sage BepInEx shell. Patches, builds the threading and config primitives,
/// watches level load/unload, and creates/disposes <see cref="SageMod"/>
/// per level. The mod owns every runtime object; the shell owns none.
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
    private MainThreadExecutor? _threads;
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
        _threads = new MainThreadExecutor(synchronizationContext, unityThreadId, _log);
        _log.Log($"captured Unity thread {unityThreadId}; synchronization context available={synchronizationContext != null}");

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

        CommandLine.AddCommand("ai", new AiConsoleCommand(new CommandFactory(() => _mod)));
        _log.Log("registered 'ai' console command");

        _log.Log($"Loaded {ModName} v{ModVersion} (core {CoreAssembly.Version})");
    }

    private void OnDestroy()
    {
        UnloadMod();
    }

    /// <summary>Builds the mod for the loaded level. No-op when built.</summary>
    private void LoadMod()
    {
        if (_mod != null)
        {
            return;
        }

        if (_settings is null || _threads is null || _log is null)
        {
            return;
        }

        _mod = new SageMod(_settings, BepInEx.Paths.ConfigPath, _threads, _log);
        _mod.Load();
    }

    /// <summary>Tears down the mod for the unloading level. No-op when down.</summary>
    private void UnloadMod()
    {
        try
        {
            _mod?.Unload();
        }
        catch (Exception ex)
        {
            _log?.Log(ex.ToString());
        }

        _mod = null;
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
