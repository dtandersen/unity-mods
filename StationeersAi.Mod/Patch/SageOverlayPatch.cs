using HarmonyLib;

namespace StationeersAi.Mod.Patch;

/// <summary>
/// Draws the Sage window inside the game's Dear ImGui frame, right after the
/// console draws, so it shares the frame, style, and fonts. Hooks the draw
/// itself rather than the overlay end, which already submitted the frame.
/// </summary>
[HarmonyPatch(typeof(Assets.Scripts.ConsoleWindow), "Draw")]
public static class SageOverlayPatch
{
    public static void Postfix()
    {
        SageImGuiWindow.DrawAll();
    }
}
