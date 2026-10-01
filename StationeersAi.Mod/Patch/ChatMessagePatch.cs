using HarmonyLib;
using NetChatMessage = Assets.Scripts.Networking.ChatMessage;

namespace StationeersAi.Mod.Patch;

/// <summary>
/// Watches locally displayed chat. <c>ChatMessage.PrintToConsole</c> runs for
/// every chat message shown on this machine (own messages echo back through
/// the server relay), so it is the single interception point for mentions on
/// clients, hosts, servers, and single-player alike.
/// </summary>
[HarmonyPatch(typeof(NetChatMessage), nameof(NetChatMessage.PrintToConsole))]
public static class ChatMessagePatch
{
    public static void Postfix(NetChatMessage __instance)
    {
        BepInExPlugin.OnChatMessage(__instance);
    }
}
