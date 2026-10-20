using System;
using Caev;
using Sage.Core;
using Sage.Ai;
using Sage.Game.View;
using Sage.Presentation;
using Sage.Game.Server;
using Sage.Window;
using UnityEngine;
using Logger = Sage.Core.Logger;

namespace Sage.Game;

/// <summary>
/// Level lifecycle for Sage: attaches the window, serves HTTP, and hands chat
/// lines to the presenter while a level runs. The shell hands this the settings,
/// the agent factory, the view, the main thread, and the log; the agent gateway,
/// the chat view, and the presenter are built once, on the first load, and kept
/// across levels.
/// </summary>
public sealed class SageMod
{
    private readonly SageConfig _settings;
    private readonly SageAgentFactory _agents;
    private readonly SwitchingSageView _view;
    private readonly MainThreadExecutor _threads;
    private readonly Logger _log;
    private readonly WindowModel _model = new WindowModel(200);

    private bool _loaded;
    private AgentGateway? _gateway;
    private SagePresenter? _presenter;
    private GameObject? _holder;
    private SageImGuiWindow? _window;
    private SageHttpServer? _httpServer;

    public SageMod(
        SageConfig settings,
        SageAgentFactory agents,
        SwitchingSageView view,
        MainThreadExecutor threads,
        Logger log)
    {
        _settings = settings;
        _agents = agents;
        _view = view;
        _threads = threads;
        _log = log;
    }

    /// <summary>The window presenter, once the level has been loaded.</summary>
    public SagePresenter? Presenter => _presenter;

    /// <summary>
    /// Runs one console line. The presenter belongs to the loaded mod, so a
    /// command issued before any level says so instead of guessing.
    /// </summary>
    public string OnConsoleCommand(string[] args)
    {
        var presenter = _presenter;
        return presenter != null
            ? presenter.OnConsoleCommand(args)
            : "game data is not loaded yet.";
    }

    public bool IsLoaded => _loaded;

    /// <summary>Builds window and server for the loaded level. No-op when loaded.</summary>
    public void Load()
    {
        if (_loaded)
        {
            _log.Warn("SageMod.Load called when already loaded");
            return;
        }

        if (!_settings.ModEnabled.Value)
        {
            return;
        }

        _log.Info("Loading SageMod...");

        EnsureAgent();
        EnsureWindow();
        EnsureHttpServer();
        _loaded = true;
        _log.Info("SageMod loaded");
    }

    /// <summary>Tears down window and server. The shell only calls this leaving a world.</summary>
    public void Unload()
    {
        if (!_loaded)
        {
            _log.Warn("SageMod.Unload called, but mod not loaded.");
            return;
        }

        _log.Info("Unloading SageMod...");
        _loaded = false;

        try
        {
            _httpServer?.Dispose();
        }
        catch (Exception ex)
        {
            _log.Error(ex);
        }

        _httpServer = null;

        try
        {
            if (_holder != null)
            {
                UnityEngine.Object.Destroy(_holder);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex);
        }

        _view.Show(null);
        _holder = null;
        _window = null;
        _log.Info("SageMod unloaded");
    }

    /// <summary>
    /// Called for every chat message displayed on this machine. Hands the line and
    /// the values routing needs to the presenter, which decides nothing about
    /// whether the line asks something - <see cref="ChatRouting"/> does that.
    /// Runs on the Unity main thread.
    /// </summary>
    public void HandleChatMessage(Assets.Scripts.Networking.ChatMessage message)
    {
        if (!_loaded || message == null || !_settings.ModEnabled.Value)
        {
            return;
        }

        var presenter = _presenter;
        if (presenter == null)
        {
            return;
        }

        presenter.OnChatLine(
            new ChatLine(message.DisplayName, message.ChatText),
            new ChatRoutingRules(
                _agents.AssistantName,
                _settings.MentionTrigger.Value,
                _settings.RespondToMentions.Value,
                OthersInGame()));
    }

    /// <summary>
    /// Starts the loopback HTTP server for outside tools such as Pi when
    /// <c>Server/Enabled</c> is on. Each request reuses the same live
    /// repositories the chat assistant reads.
    /// </summary>
    private void EnsureHttpServer()
    {
        try
        {
            if (!_settings.ServerEnabled.Value)
            {
                return;
            }

            if (_httpServer != null && _httpServer.IsAlive)
            {
                return;
            }

            try
            {
                _httpServer?.Dispose();
            }
            catch (Exception)
            {
            }

            _httpServer = new SageHttpServer(
                () => _agents.Commands,
                _threads,
                _log,
                () => _settings.ServerPort.Value,
                () => _settings.ServerToken.Value);
            _httpServer.Start();
        }
        catch (Exception ex)
        {
            _log.Info($"Sage HTTP server did not start: {ex.Message}");
            _httpServer = null;
        }
    }

    /// <summary>
    /// Builds the window holder and view, then points the presenter at it.
    /// Unity's fake null counts destroyed objects.
    /// </summary>
    private void EnsureWindow()
    {
        if (_window != null)
        {
            return;
        }

        _holder = new GameObject("SageWindow");
        UnityEngine.Object.DontDestroyOnLoad(_holder);
        _window = _holder.AddComponent<SageImGuiWindow>();
        _window.Logger = _log;
        _window.WindowToggle = () => _settings.WindowToggle.Value;
        _view.Show(_window);
        _presenter?.Refresh();
        _log.Info($"Sage window ready ({_settings.WindowToggle.Value})");
    }

    /// <summary>
    /// Whether every chat message is answered directly: local games and
    /// empty servers have no audience to disturb, so no mention prefix is
    /// needed. Fails closed to mentions only.
    /// </summary>
    /// <summary>
    /// Whether another player is in the game, so only a mention addresses the
    /// agent. A local game has nobody else. When the count cannot be read, say
    /// somebody is there: an unasked question is worse than an unanswered one.
    /// </summary>
    private bool OthersInGame()
    {
        try
        {
            if (!Assets.Scripts.Networking.NetworkManager.IsClient)
            {
                return false;
            }

            return Assets.Scripts.Networking.NetworkManager.TotalPlayersInGame > 1;
        }
        catch (Exception)
        {
            return true;
        }
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
        {
            return value;
        }

        return value.Substring(0, max) + "...";
    }

    /// <summary>
    /// Builds the agent once for this mod: the gateway, the chat sink that posts its
    /// answers, the window presenter, and the mention responder. A level is loaded
    /// here, so no turn has to ask whether game data exists.
    /// </summary>
    private void EnsureAgent()
    {
        if (_gateway != null)
        {
            return;
        }

        _gateway = _agents.BuildGateway();
        _presenter = new SagePresenter(
            _model,
            _view,
            new ChatView(_threads, _log),
            _gateway,
            _agents,
            _agents.PlayerName,
            () => _agents.AssistantName,
            _log,
            () => _settings.ShowThinking.Value,
            () => _settings.ShowToolCalls.Value,
            () => _settings.ShowToolResults.Value,
            () => _agents.ConversationOrNull,
            _threads.Run);
        _log.Info("Sage gateway ready (one agent for this mod)");
    }
}
