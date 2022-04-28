using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishnetNetworkHelper : NetworkBehaviour
{
    private void Start()
    {
        transform.SetParent(NetworkManager.transform);
    }

    #region Send / Recieve Player Data

    [Client]
    public  void SendDataToNetworkPlayer(NetworkPlayer OtherPlayer, string YourDataJSON)
    {
        SendDataToNetworkPlayer_ServerRPC(OtherPlayer.Owner, YourDataJSON);
    }

    [ServerRpc]
    private void SendDataToNetworkPlayer_ServerRPC(NetworkConnection OtherPlayerConn, string YourDataJSON)
    {
        RecieveDataToNetworkPlayer_TargetRPC(OtherPlayerConn, YourDataJSON);
    }

    [TargetRpc]
    private void RecieveDataToNetworkPlayer_TargetRPC(NetworkConnection OtherPlayerConn, string YourDataJSON)
    {
        PlayerData SentPlayersData = PlayerData.Deserialize(YourDataJSON);
        PlayerManager.Instance.RecievePlayerData(SentPlayersData);
    }

    #endregion
}
