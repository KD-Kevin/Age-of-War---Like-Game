using AgeOfWar.Core;
using AgeOfWar.Data;
using AgeOfWar.UI;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AgeOfWar.Networking
{
    public class FishnetNetworkHelper : NetworkBehaviour
    {
        public static FishnetNetworkHelper Instance = null;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
            PlayerManager.Instance.NetworkHelper = this;
        }

        private void Start()
        {
            transform.SetParent(NetworkManager.transform);
        }

        #region Send / Recieve Player Data

        [Client]
        public void SendDataToNetworkPlayer(NetworkPlayer OtherPlayer, string YourDataJSON)
        {
            //Debug.Log("Send Player Data 2");
            SendDataToNetworkPlayer_ServerRPC(OtherPlayer.Owner, YourDataJSON);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendDataToNetworkPlayer_ServerRPC(NetworkConnection OtherPlayerConn, string YourDataJSON)
        {
            //Debug.Log("Send Player Data 3");
            RecieveDataToNetworkPlayer_TargetRPC(OtherPlayerConn, YourDataJSON);
        }

        [TargetRpc]
        private void RecieveDataToNetworkPlayer_TargetRPC(NetworkConnection OtherPlayerConn, string YourDataJSON)
        {
            //Debug.Log("Send Player Data 4"); 
            PlayerData SentPlayersData = PlayerData.Deserialize(YourDataJSON);
            PlayerManager.Instance.RecievePlayerData(SentPlayersData);
        }

        #endregion

        #region Custom Game Ready

        [Client]
        public void SendReady(NetworkPlayer YourPlayer, bool Ready)
        {
            //Debug.Log("Send Player Data 2");
            SendReady_ServerRPC(YourPlayer.Owner, Ready);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendReady_ServerRPC(NetworkConnection OtherPlayerConn, bool Ready)
        {
            //Debug.Log("Send Player Data 3");
            SendReady_ObserverRPC(OtherPlayerConn, Ready);
        }

        [ObserversRpc]
        private void SendReady_ObserverRPC(NetworkConnection OtherPlayerConn, bool Ready)
        {
            //Debug.Log("Send Player Data 4"); 
            if (OtherPlayerConn.ClientId == PlayerManager.Instance.LocalPlayer.OwnerId)
            {
                CustomGamePage.Instance.LocalPlayerReady(Ready);
            }
            else
            {
                CustomGamePage.Instance.OtherPlayerReady(Ready);
            }
        }

        #endregion

        #region Player Ready RPC

        [Client]
        public void SendReady(NetworkPlayer OtherPlayer)
        {
            //Debug.Log("Send Player Data 2");
            SendReady_ServerRPC(OtherPlayer.Owner);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendReady_ServerRPC(NetworkConnection YourConnection)
        {
            //Debug.Log("Send Player Data 3");
            foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
            {
                if (player.OwnerId == YourConnection.ClientId)
                {
                    player.ReadyToStart = true;
                }
            }
        }

        #endregion
    }
}
