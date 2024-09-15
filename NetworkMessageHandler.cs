using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace MyLethalCompanyPlugin
{
    public static class NetworkMessageHandler
    {
        private const short PluginCheckMessage = 1001;

        public static void RegisterHandlers()
        {
            if (NetworkServer.active) // If this is the server (host)
            {
                NetworkServer.RegisterHandler(PluginCheckMessage, OnPluginCheckResponse);
                SendPluginCheckMessage();
            }
            else if (NetworkClient.active) // If this is a client
            {
                NetworkManager.singleton.client.RegisterHandler(PluginCheckMessage, OnPluginCheckRequest);
            }
        }

        private static void SendPluginCheckMessage()
        {
            NetworkServer.SendToAll(PluginCheckMessage, new PluginCheckMessageData { IsPluginInstalled = true });
        }

        private static void OnPluginCheckRequest(NetworkMessage netMsg)
        {
            var response = new PluginCheckMessageData { IsPluginInstalled = true };
            netMsg.conn.Send(PluginCheckMessage, response);
        }

        private static void OnPluginCheckResponse(NetworkMessage netMsg)
        {
            var data = netMsg.ReadMessage<PluginCheckMessageData>();

            if (!data.IsPluginInstalled)
            {
                // Handle the case where the client does not have the plugin installed
                PreventCosmeticConfigPlugin.LoggerInstance.LogError("A client does not have the required plugin installed. Disconnecting...");
                NetworkServer.DisconnectAll();
            }
        }
    }

    public class PluginCheckMessageData : MessageBase
    {
        public bool IsPluginInstalled;
    }
}
