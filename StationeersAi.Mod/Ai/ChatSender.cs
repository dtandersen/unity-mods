using System;
using Assets.Scripts;
using Assets.Scripts.Networking;
using NetChatMessage = Assets.Scripts.Networking.ChatMessage;

namespace StationeersAi.Mod.Ai;

/// <summary>
/// Sends a chat message as the assistant, mirroring how the game sends player
/// chat (<c>InventoryManager.OnChatMessage</c>): clients forward to the
/// server (which relays to everyone, including back to us), a dedicated
/// server prints locally and relays to clients, and single-player just
/// prints locally. Everything here runs on the Unity main thread.
/// </summary>
internal static class ChatSender
{
    public static void Send(string displayName, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        // One chat message per line: the chat window renders single lines.
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                continue;
            }

            SendLine(displayName, line);
        }
    }

    private static void SendLine(string displayName, string line)
    {
        var message = new NetChatMessage
        {
            ChatText = line,
            DisplayName = displayName,
            HumanId = -1,
        };

        try
        {
            if (NetworkManager.IsClient)
            {
                NetworkClient.SendToServer(message, NetworkChannel.GeneralTraffic);
                return;
            }

            message.PrintToConsole();

            if (NetworkManager.IsServer)
            {
                NetworkServer.SendToClients(message, NetworkChannel.GeneralTraffic, -1L);
            }
        }
        catch (Exception ex)
        {
            BepInExPlugin.Log.Log($"chat send failed: {ex}");
        }
    }
}
