using System;
using StationeersAi.Core;
using StationeersAi.Game;
using Logger = StationeersAi.Core.Logger;
using System.Collections.Generic;
using BepInEx.Configuration;
using ImGuiNET;
using StationeersAi.Window;
using UnityEngine;

namespace StationeersAi.Game;

/// <summary>
/// Draggable in-game window for watching Sage, drawn in the game's own
/// Dear ImGui frame so it matches the native style and fonts. A passive view
/// over its presenter: renders the lines, status and visible flag the
/// presenter sets, and reports the toggle key as an event. No trimming, no
/// busy checks, no history of its own. Questions arrive through game chat or
/// the console command; the bar at the foot shows Idle or the thinking status.
/// </summary>
public sealed class SageImGuiWindow : MonoBehaviour, ISageView
{
    private List<string> _lines = new List<string>();
    private string _status = "Idle";
    private bool _visible;
    private bool _autoscroll;
    private bool _sized;
    private static readonly List<SageImGuiWindow> _windows = new List<SageImGuiWindow>();

    /// <summary>Trace log. Null skips logging (set by the plugin).</summary>
    public Logger? Logger { get; set; }

    /// <summary>Supplies the toggle key, read live on each press. Null disables toggling.</summary>
    public Func<KeyboardShortcut>? WindowToggle { get; set; }

    public event Action? ToggleRequested;

    public event Action? UpdateRequested;

    public void SetLines(IReadOnlyList<string> lines)
    {
        _lines = lines != null ? new List<string>(lines) : new List<string>();
        _autoscroll = true;
    }

    public void SetStatus(string status)
    {
        _status = status ?? "Idle";
    }

    public void SetVisible(bool visible)
    {
        _visible = visible;
    }

    private void Update()
    {
        if (TogglePressed())
        {
            ToggleRequested?.Invoke();
        }

        UpdateRequested?.Invoke();
    }

    /// <summary>Draws every live window. Called from the overlay patch.</summary>
    public static void DrawAll()
    {
        foreach (var window in _windows.ToArray())
        {
            if (window != null)
            {
                window.Draw();
            }
        }
    }

    private void OnEnable()
    {
        if (!_windows.Contains(this))
        {
            _windows.Add(this);
        }
    }

    private void OnDisable()
    {
        _windows.Remove(this);
    }

    private bool TogglePressed()
    {
        var provider = WindowToggle;
        if (provider == null)
        {
            return false;
        }

        var toggle = provider();
        if (toggle.MainKey == KeyCode.None || !UnityEngine.Input.GetKeyDown(toggle.MainKey))
        {
            return false;
        }

        foreach (var modifier in toggle.Modifiers)
        {
            switch (modifier)
            {
                case KeyCode.LeftControl:
                case KeyCode.RightControl:
                    if (!UnityEngine.Input.GetKey(KeyCode.LeftControl) && !UnityEngine.Input.GetKey(KeyCode.RightControl)) return false;
                    break;
                case KeyCode.LeftShift:
                case KeyCode.RightShift:
                    if (!UnityEngine.Input.GetKey(KeyCode.LeftShift) && !UnityEngine.Input.GetKey(KeyCode.RightShift)) return false;
                    break;
                case KeyCode.LeftAlt:
                case KeyCode.RightAlt:
                    if (!UnityEngine.Input.GetKey(KeyCode.LeftAlt) && !UnityEngine.Input.GetKey(KeyCode.RightAlt)) return false;
                    break;
                default:
                    if (!UnityEngine.Input.GetKey(modifier)) return false;
                    break;
            }
        }

        return true;
    }

    private void Draw()
    {
        if (!_visible)
        {
            return;
        }

        if (!_sized)
        {
            _sized = true;
            // First run only: once imgui.ini holds this window, the saved
            // position and size rule and this default is ignored.
            ImGui.SetNextWindowSize(new Vector2(640, 480), ImGuiCond.FirstUseEver);
        }

        if (!ImGui.Begin("SAGE"))
        {
            ImGui.End();
            return;
        }

        var lines = _lines;
        if (ImGui.BeginChild("scroll", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()), true))
        {
            foreach (var line in lines)
            {
                ImGui.TextWrapped(Escape(line));
            }

            if (_autoscroll)
            {
                ImGui.SetScrollHereY(1f);
                _autoscroll = false;
            }
        }

        ImGui.EndChild();

        ImGui.Separator();
        ImGui.TextWrapped(Escape(_status));

        ImGui.End();
    }

    /// <summary>TextWrapped treats its text as a format string; escape it.</summary>
    private static string Escape(string line)
    {
        return line != null && line.Contains("%") ? line.Replace("%", "%%") : line ?? string.Empty;
    }
}
