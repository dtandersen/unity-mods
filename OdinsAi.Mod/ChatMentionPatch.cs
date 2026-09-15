using HarmonyLib;

namespace OdinsAi.Mod
{
    /// <summary>
    /// Watches incoming chat for a mention. Every client receives every chat
    /// message, so the machine running Odin sees questions from all players.
    /// </summary>
    [HarmonyPatch(typeof(Chat), nameof(Chat.OnNewChatMessage))]
    internal static class ChatMentionPatch
    {
        [HarmonyPostfix]
        private static void Postfix(string text)
        {
            BepInExPlugin.OnPlayerChat(text);
        }
    }
}