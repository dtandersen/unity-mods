using HarmonyLib;

namespace Sage.Game.Patch;

/// <summary>
/// Unloads the mod when the player leaves the game, mirroring the api-mod's
/// LeaveGame watcher. The BepInEx shell forwards to the mod, which tears
/// down its window and server.
/// </summary>
[HarmonyPatch(typeof(Assets.Scripts.GameManager), "LeaveGame")]
public static class LeaveGamePatch
{
    public static void Postfix()
    {
        PatchHooks.LeaveGame?.Invoke();
    }
}
