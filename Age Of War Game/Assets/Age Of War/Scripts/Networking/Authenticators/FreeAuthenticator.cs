using AgeOfWar.Core;
using FishNet.Authenticating;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Logging;
using FishNet.Transporting;
using System;
using UnityEngine;


namespace AgeOfWar.Networking
{
    public class FreeAuthenticator : Authenticator
    {
        public override event Action<NetworkConnection, bool> OnAuthenticationResult;


        public override void InitializeOnce(NetworkManager networkManager)
        {
            base.InitializeOnce(networkManager);

            //Listen for connection state change as client.
            base.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
            base.NetworkManager.ServerManager.Authenticator = this;
            //Listen for broadcast from client. Be sure to set requireAuthentication to false.
            base.NetworkManager.ServerManager.RegisterBroadcast<FreeConnectBroadcast>(OnFreeconnectBroadcast, false);
            //Listen to response from server.
            base.NetworkManager.ClientManager.RegisterBroadcast<FreeResponseBroadcast>(OnResponseBroadcast);
        }

        /// <summary>
        /// Called when a connection state changes for the local client.
        /// </summary>
        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState != LocalConnectionStates.Started)
                return;

            FreeConnectBroadcast pb = new FreeConnectBroadcast()
            {
                ConnectingConnectionUsername = PlayerManager.Instance.LocalPlayerData.Data.UserName
            };

            base.NetworkManager.ClientManager.Broadcast(pb);
        }


        /// <summary>
        /// Received on server when a client sends the password broadcast message.
        /// </summary>
        /// <param name="conn">Connection sending broadcast.</param>
        /// <param name="pb"></param>
        private void OnFreeconnectBroadcast(NetworkConnection conn, FreeConnectBroadcast pb)
        {
            /* If client is already authenticated this could be an attack. Connections
             * are removed when a client disconnects so there is no reason they should
             * already be considered authenticated. */
            if (conn.Authenticated)
            {
                conn.Disconnect(true);
                return;
            }

            //Invoke result. This is handled internally to complete the connection or kick client.
            OnAuthenticationResult?.Invoke(conn, true);
            /* Tell client if they authenticated or not. This is
             * entirely optional but does demonstrate that you can send
             * broadcasts to client on pass or fail. */
            FreeResponseBroadcast rb = new FreeResponseBroadcast()
            {
                ConnectingConnectionUsername = pb.ConnectingConnectionUsername
            };
            base.NetworkManager.ServerManager.Broadcast(conn, rb, false);
        }

        /// <summary>
        /// Received on client after server sends an authentication response.
        /// </summary>
        /// <param name="rb"></param>
        private void OnResponseBroadcast(FreeResponseBroadcast rb)
        {
            if (NetworkManager.CanLog(LoggingType.Common))
            {
                Debug.Log($"{rb.ConnectingConnectionUsername} Connected");
            }
        }

        public struct FreeConnectBroadcast : IBroadcast
        {
            public string ConnectingConnectionUsername;
        }

        public struct FreeResponseBroadcast : IBroadcast
        {
            public string ConnectingConnectionUsername;
        }
    }
}
