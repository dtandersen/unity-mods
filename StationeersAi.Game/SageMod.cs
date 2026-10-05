using System;
using Caev;
using StationeersAi.Ai;
using StationeersAi.Game.Ai;
using StationeersAi.Game.Server;
using StationeersAi.Window;
using UnityEngine;
using Logger = StationeersAi.Core.Logger;

namespace StationeersAi.Game;

/// <summary>
/// Level lifecycle for Sage: attaches the window, serves HTTP, and answers
/// chat mentions while a level runs. The shell composes the assistant
/// factory, view, and presenter; the mod only plugs them into the level.
/// </summary>
public sealed class SageMod
{
    private readonly SageConfig _settings;
    private readonly SageAssistantFactory _assistants;
    private readonly SwitchingSageView _view;
    private readonly SagePresenter _presenter;
    private readonly MainThreadExecutor _threads;
    private readonly Logger _log;

    private bool _loaded;
    private MentionResponder? _mentionResponder;
    private GameObject? _holder;
    private SageImGuiWindow? _window;
    private SageHttpServer? _httpServer;

    public SageMod(
        SageConfig settings,
        SageAssistantFactory assistants,
        SwitchingSageView view,
        SagePresenter presenter,
        MainThreadExecutor threads,
        Logger log)
    {
        _settings = settings;
        _assistants = assistants;
        _view = view;
        _presenter = presenter;
        _threads = threads;
        _log = log;
    }

    public bool IsLoaded => _loaded;

    /// <summary>Builds window and server for the loaded level. No-op when loaded.</summary>
    public void Load()
    {
        _log.Info(GameStateLine());
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

        EnsureWindow();
        EnsureHttpServer();
        _loaded = true;
        _log.Info("SageMod loaded");
    }

    /// <summary>Tears down window and server. The shell only calls this leaving a world.</summary>
    public void Unload()
    {
        _log.Info(GameStateLine());
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
    /// Called for every chat message displayed on this machine. Mentions
    /// are always answered; other messages are answered directly in local
    /// games and empty servers. The question is echoed to the Sage window
    /// and answered there through the window presenter. Runs on the Unity
    /// main thread, so the game-data snapshot is safe to build. Ignored
    /// while another turn is running.
    /// </summary>
    public void HandleChatMessage(Assets.Scripts.Networking.ChatMessage message)
    {
        if (!_loaded || message == null)
        {
            return;
        }

        if (!_settings.ModEnabled.Value || !_settings.RespondToMentions.Value)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(message.ChatText))
        {
            return;
        }

        var assistantName = _assistants.AssistantName;

        // Skip our own answers echoing back through the server relay.
        if (message.DisplayName != null &&
            message.DisplayName.Equals(assistantName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _log.Info($"chat seen from '{message.DisplayName}': {Truncate(message.ChatText, 80)}");

        var mentioned = Mention.TryParse(_settings.MentionTrigger.Value, message.ChatText, out var prompt);
        if (!mentioned)
        {
            if (!AnswersDirectly())
            {
                return;
            }

            prompt = message.ChatText.Trim();
            _log.Info($"direct chat asking: {Truncate(prompt, 80)}");
        }
        else
        {
            _log.Info($"mention matched, asking: {Truncate(prompt, 80)}");
        }

        EnsureWindow();

        var responder = MentionResponderInstance();
        if (responder.IsBusy)
        {
            _log.Info("mention ignored: Sage is already answering");
            return;
        }

        _presenter.Show();
        var speaker = string.IsNullOrWhiteSpace(message.DisplayName)
            ? "player"
            : message.DisplayName;
        _presenter.Write($"{speaker}: {prompt}");
        _ = responder.HandleAsync(prompt, _assistants.MentionConversation(message.DisplayName));
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
                _assistants.BuildSageData,
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
        _presenter.Refresh();
        _log.Info($"Sage window ready ({_settings.WindowToggle.Value})");
    }

    /// <summary>
    /// Whether every chat message is answered directly: local games and
    /// empty servers have no audience to disturb, so no mention prefix is
    /// needed. Fails closed to mentions only.
    /// </summary>
    private bool AnswersDirectly()
    {
        try
        {
            if (!Assets.Scripts.Networking.NetworkManager.IsClient)
            {
                return true;
            }

            return Assets.Scripts.Networking.NetworkManager.TotalPlayersInGame <= 1;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string GameStateLine()
    {
        try
        {
            return $"GameState: {Assets.Scripts.GameManager.GameState}";
        }
        catch (Exception)
        {
            return "GameState: unknown";
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

    private MentionResponder MentionResponderInstance()
    {
        EnsureWindow();

        return _mentionResponder ??= new MentionResponder(
            new FanoutChatSink(_presenter, new ChatMessageSink(_threads, _log, () => _assistants.AssistantName)),
            activity => _assistants.BuildAssistant(activity),
            _assistants.AssistantName,
            _log);
    }
}
