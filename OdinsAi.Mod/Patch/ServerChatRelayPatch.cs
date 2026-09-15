using HarmonyLib;

namespace OdinsAi.Mod.Patch
{
    /// <summary>
    /// On a dedicated server the chat handlers never run: the server only relays
    /// chat RPCs on their way between clients. This watches the relay so a
    /// server-side Odin can read what players type.
    /// </summary>
    [HarmonyPatch(typeof(ZRoutedRpc), "RouteRPC")]
    internal static class ServerChatRelayPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ZRoutedRpc.RoutedRPCData rpcData)
        {
            BepInExPlugin.OnServerRoutedRpc(rpcData);
        }
    }
}