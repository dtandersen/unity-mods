// Minimal ImGuiNET fakes (real API ships inside the game's RG.ImGui.dll).
// Only the members SageImGuiWindow calls.
using UnityEngine;

namespace ImGuiNET
{
    public enum ImGuiCond { FirstUseEver }

    public static class ImGui
    {
        public static void SetNextWindowSize(Vector2 size, ImGuiCond cond) { }
        public static bool Begin(string name) => false;
        public static void End() { }
        public static bool BeginChild(string id, Vector2 size, bool border) => false;
        public static void EndChild() { }
        public static void TextWrapped(string text) { }
        public static void Separator() { }
        public static float GetFrameHeightWithSpacing() => 0f;
        public static void SetScrollHereY(float ratio) { }
    }
}
