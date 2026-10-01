using System.Collections.Generic;
using UnityEngine;

namespace StationeersAi.Mod
{
    /// <summary>
    /// Draggable in-game window for talking to Sage directly: scrollable
    /// history, a text field, and send on Enter. All Unity calls happen on the
    /// main thread; answers arrive through <see cref="AddLine"/> under lock.
    /// </summary>
    public sealed class SageWindow : MonoBehaviour
    {
        private const int WindowId = 0x5A47;
        private const int MaxHistoryLines = 200;

        private readonly List<string> _history = new List<string>();
        private readonly object _gate = new object();

        private Rect _area = new Rect(Screen.width - 640, 80, 600, 420);
        private Vector2 _scroll;
        private string _input = string.Empty;
        private GUIStyle _wrapped;
        private bool _focusInput;

        public bool Visible { get; set; }

        public bool Busy { get; set; }

        /// <summary>Thread-safe: called by the presenter from any thread.</summary>
        public void AddLine(string line)
        {
            lock (_gate)
            {
                _history.Add(line ?? string.Empty);
                while (_history.Count > MaxHistoryLines)
                {
                    _history.RemoveAt(0);
                }
            }

            _scroll.y = float.MaxValue;
        }

        private void Update()
        {
            if (BepInExPlugin.windowToggle != null && BepInExPlugin.windowToggle.Value.IsDown())
            {
                Visible = !Visible;
                _focusInput = Visible;
            }
        }

        private void OnGUI()
        {
            if (!Visible)
            {
                return;
            }

            if (_wrapped == null)
            {
                _wrapped = new GUIStyle(GUI.skin.label) { wordWrap = true };
            }

            _area = GUI.Window(WindowId, _area, DrawWindow, "SAGE");
        }

        private void DrawWindow(int id)
        {
            List<string> lines;
            lock (_gate)
            {
                lines = new List<string>(_history);
            }

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
            foreach (var line in lines)
            {
                GUILayout.Label(line, _wrapped);
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("SageInput");
            _input = GUILayout.TextField(_input, GUILayout.ExpandWidth(true));
            var send = GUILayout.Button(Busy ? "..." : "Send", GUILayout.Width(64));
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

            if (fired && !Busy && !string.IsNullOrWhiteSpace(_input))
            {
                var prompt = _input.Trim();
                _input = string.Empty;
                AddLine("You: " + prompt);
                BepInExPlugin.context.AskInWindow(this, prompt);
            }

            GUI.DragWindow();
        }
    }
}
