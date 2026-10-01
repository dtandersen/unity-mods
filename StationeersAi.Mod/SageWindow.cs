using System;
using System.Collections.Generic;
using StationeersAi.Window;
using UnityEngine;

namespace StationeersAi.Mod;

/// <summary>
/// Draggable in-game window for talking to Sage. A passive view over its
/// <see cref="SagePresenter"/>: renders the lines, input, busy and visible
/// flags the presenter sets, and reports the toggle key, the Send button
/// and Enter as events. No trimming, no busy checks, no history of its own.
/// </summary>
public sealed class SageWindow : MonoBehaviour, ISageView
{
    private const int WindowId = 0x5A47;

    private Rect _area = new Rect(Screen.width - 640, 80, 600, 420);
    private Vector2 _scroll;
    private List<string> _lines = new List<string>();
    private string _input = string.Empty;
    private bool _visible;
    private bool _busy;
    private bool _autoscroll;
    private GUIStyle _wrapped;
    private GUIStyle _windowStyle;
    private Texture2D _background;
    private bool _focusInput;
    private bool _probed;

    public string Input
    {
        get => _input;
        set => _input = value ?? string.Empty;
    }

    /// <summary>Trace log. Null skips logging (set by the plugin).</summary>
    public BepinExLogger Logger { get; set; }

    /// <summary>Supplies the toggle key, read live on each press. Null disables toggling.</summary>
    public Func<BepInEx.Configuration.KeyboardShortcut> WindowToggle { get; set; }

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

    private bool TogglePressed()
    {
        var shot = Event.current;
        if (shot == null || shot.type != EventType.KeyDown || shot.keyCode == KeyCode.None)
        {
            return false;
        }

        var toggleProvider = WindowToggle;
        var toggle = toggleProvider != null
            ? toggleProvider()
            : default(BepInEx.Configuration.KeyboardShortcut);
        if (shot.keyCode != toggle.MainKey || toggle.MainKey == KeyCode.None)
        {
            return false;
        }

        foreach (var modifier in toggle.Modifiers)
        {
            switch (modifier)
            {
                case KeyCode.LeftControl:
                case KeyCode.RightControl:
                    if (!shot.control) return false;
                    break;
                case KeyCode.LeftShift:
                case KeyCode.RightShift:
                    if (!shot.shift) return false;
                    break;
                case KeyCode.LeftAlt:
                case KeyCode.RightAlt:
                    if (!shot.alt) return false;
                    break;
                default:
                    if (!UnityEngine.Input.GetKey(modifier)) return false;
                    break;
            }
        }

        return true;
    }

    private void OnGUI()
    {
        if (!_probed)
        {
            _probed = true;
            Logger?.Log("Sage window OnGUI live");
        }

        if (TogglePressed())
        {
            ToggleRequested?.Invoke();
            Event.current.Use();
            if (!_visible)
            {
                GUI.FocusControl(null);
            }
        }

        if (!_visible)
        {
            return;
        }

        if (_wrapped == null)
        {
            _wrapped = new GUIStyle(GUI.skin.label) { wordWrap = true };
        }

        if (_windowStyle == null)
        {
            _background = new Texture2D(1, 1);
            _background.SetPixel(0, 0, new Color(0.06f, 0.06f, 0.06f, 1f));
            _background.Apply();
            _windowStyle = new GUIStyle(GUI.skin.window);
            _windowStyle.normal.background = _background;
            _windowStyle.onNormal.background = _background;
            _windowStyle.hover.background = _background;
            _windowStyle.onHover.background = _background;
            _windowStyle.focused.background = _background;
            _windowStyle.onFocused.background = _background;
        }

        _area = GUI.Window(WindowId, _area, DrawWindow, "SAGE", _windowStyle);
    }

    private void DrawWindow(int id)
    {
        if (_autoscroll)
        {
            _scroll.y = float.MaxValue;
            _autoscroll = false;
        }

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
        foreach (var line in _lines)
        {
            GUILayout.Label(line, _wrapped);
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("SageInput");
        _input = GUILayout.TextField(_input ?? string.Empty, GUILayout.ExpandWidth(true));
        var send = GUILayout.Button(_busy ? "..." : "Send", GUILayout.Width(64));
        GUILayout.EndHorizontal();

        if (_focusInput)
        {
            GUI.FocusControl("SageInput");
            _focusInput = false;
        }

        var fired = send;
        var shot = Event.current;
        if (!fired &&
            shot != null &&
            shot.isKey &&
            (shot.keyCode == KeyCode.Return || shot.keyCode == KeyCode.KeypadEnter) &&
            GUI.GetNameOfFocusedControl() == "SageInput")
        {
            fired = true;
            shot.Use();
        }

        if (fired)
        {
            SubmitRequested?.Invoke();
        }

        GUI.DragWindow();
    }
}
