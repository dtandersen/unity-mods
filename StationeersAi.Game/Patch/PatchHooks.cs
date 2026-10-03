using System;
using NetChatMessage = Assets.Scripts.Networking.ChatMessage;

namespace StationeersAi.Game.Patch;

/// <summary>
/// Adapter-side hooks the Harmony patches call. The BepInEx shell assigns
/// these at boot; the patches never touch shell types, so the adapter
/// references nothing outside game, Unity, and core libraries.
/// </summary>
public static class PatchHooks
{
    public static Action<NetChatMessage> ChatMessage { get; set; }

    public static Action LeaveGame { get; set; }
}
