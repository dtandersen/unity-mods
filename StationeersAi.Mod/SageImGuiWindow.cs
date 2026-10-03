using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using ImGuiNET;
using ImGuiNET.Unity;
using StationeersAi.Window;
using UnityEngine;

namespace StationeersAi.Mod;

/// <summary>
/// Draggable in-game window for talking to Sage, drawn in the game's own
/// Dear ImGui frame so it matches the native style and fonts. A passive view
/// over its presenter: renders the lines, input, busy and visible flags the
/// presenter sets, and reports the toggle key, the Send button and Enter as
/// events. No trimming, no busy checks, no history of its own.
/// </summary>
public sealed class SageImGuiWindow : MonoBehaviour, ISageView
{
    private List<string> _lines = new List<string>();
    private string _input = string.Empty;
    private bool _visible;
    private bool _busy;
    private bool _autoscroll;
    private bool _sized;
    private bool _focusInput;
    private DearImGui _dearImGui;
    private float _findAfter;

    public string Input
    {
        get => _input;
        set => _input = value ?? string.Empty;
    }

    /// <summary>Trace log. Null skips logging (set by the plugin).</summary>
    public BepinExLogger Logger { get; set; }

    /// <summary>Supplies the toggle key, read live on each press. Null disables toggling.</summary>
    public Func<KeyboardShortcut> WindowToggle { get; set; }

    public event Action SubmitRequested;

    public event Action ToggleRequested;

    public void SetLines(IReadOnlyList<string> lines)
    {
        _lines = lines != null ? new List<string>(lines) : new List<string>();
        _autoscroll = true;
    }

    public void SetBusy(bool busy)
    {
        _busy = busy;
    }

    public void SetVisible(bool visible)
    {
        _visible = visible;
    }

    public void RequestFocus()
    {
        _focusInput = true;
    }

    private void Update()
    {
        if (_dearImGui == null && Time.time >= _findAfter)
        {
            _findAfter = Time.time + 1f;
            _dearImGui = FindFirstObjectByType<DearImGui>();
            if (_dearImGui != null)
            {
                _dearImGui.Layout += Draw;
            }
        }

        if (TogglePressed())
        {
            ToggleRequested?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (_dearImGui != null)
        {
            _dearImGui.Layout -= Draw;
        }
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
            ImGui.SetNextWindowSize(new Vector2(600, 420));
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

        if (_focusInput)
        {
            ImGui.SetKeyboardFocusHere();
            _focusInput = false;
        }

        var input = _input ?? string.Empty;
        var submitted = ImGui.InputText("##sageinput", ref input, 1024, ImGuiInputTextFlags.EnterReturnsTrue);
        _input = input;

        ImGui.SameLine();
        if (ImGui.Button(_busy ? "..." : "Send"))
        {
            submitted = true;
        }

        if (submitted)
        {
            SubmitRequested?.Invoke();
        }

        ImGui.End();
    }

    /// <summary>TextWrapped treats its text as a format string; escape it.</summary>
    private static string Escape(string line)
    {
        return line != null && line.Contains("%") ? line.Replace("%", "%%") : line ?? string.Empty;
    }
}
