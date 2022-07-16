using FishNet.Object;
using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using DarkRift.Client;
using DarkRift;
using AgeOfWar.Core;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar(Channel = Channel.Reliable, OnChange = nameof(PlayerIDChanged))]
    public ushort PlayerID = 0;
    public bool IsLocalPlayer = false;
    [SyncVar]
    public bool ReadyToStart = false;
    [SyncVar(Channel = Channel.Unreliable, OnChange = nameof(PlayerNameChanged))]
    public string UserName = "Guest";

    private void OnDestroy()
    {
        PlayerManager.Instance.ConnectedPlayers.Remove(PlayerID);
    }

    private void Start()
    {
        transform.SetParent(PlayerManager.Instance.transform);

        Debug.Log($"Initialize Network Player -> Owner ID: {Owner.ClientId}");
        if (IsServer)
        {
            PlayerID = (ushort)(PlayerManager.Instance.ConnectedPlayers.Count + 1);
            if (!PlayerManager.Instance.ConnectedPlayers.ContainsKey(PlayerID))
            {
                PlayerManager.Instance.ConnectedPlayers.Add(PlayerID, this);
            }
            else
            {
                if (PlayerManager.Instance.ConnectedPlayers[PlayerID] != this)
                {
                    NetworkPlayer RemovedPlayer = PlayerManager.Instance.ConnectedPlayers[PlayerID];
                    PlayerManager.Instance.ConnectedPlayers.Remove(PlayerID);
                    PlayerManager.Instance.ConnectedPlayers.Add(PlayerID, this);

                    Debug.Log($"Player {RemovedPlayer.name}#{RemovedPlayer.GetInstanceID()} replaced with {UserName}#{RemovedPlayer.GetInstanceID()}");
                }
            }
        }

        IsLocalPlayer = Owner.IsLocalClient;
        if (IsLocalPlayer)
        {
            PlayerManager.Instance.LocalPlayer = this;
        }
        if (!IsLocalPlayer)
        {
            Invoke(nameof(SendPlayerDataDelayed), 0.1f);
        }
        else if (IsOwner && !IsServer)
        {
            // Update Name
            UpdateUsername(PlayerManager.Instance.LocalPlayerData.Data.UserName);
        }
        else if (IsServer)
        {
            // Update Name
            UserName = PlayerManager.Instance.LocalPlayerData.Data.UserName;
        }
    }

    private void OnDisable()
    {
        if (PlayerManager.Instance.ConnectedPlayers.ContainsKey(PlayerID))
        {
            PlayerManager.Instance.ConnectedPlayers.Remove(PlayerID);
        }
    }

    private void SendPlayerDataDelayed()
    {
        if (PlayerID <= 0)
        {
            Invoke(nameof(SendPlayerDataDelayed), 0.1f);
            return;
        }

        if (PlayerManager.Instance.NetworkHelper == null)
        {
            PlayerManager.Instance.SpawnFishnetNetworkHelper();
            Invoke(nameof(SendPlayerDataDelayed), 0.1f);
            return;
        }

        PlayerManager.Instance.OtherNetworkPlayerConnected(this);
    }

    #region Variables Changed RPC

    #region Fishnet

    private void PlayerIDChanged(ushort PrevID, ushort NewID, bool asServer)
    {
        if (PrevID == NewID)
        {
            return;
        }

        // Gameobject should reflect the correct ID
        if (IsOwner)
        {
            name = "Local ";
        }
        else
        {
            name = "Connecting ";
        }
        name += $"Player {PlayerID}: ({UserName})";
        if (NewID == 1)
        {
            name += $" (Host)";
        }

        if (!PlayerManager.Instance.ConnectedPlayers.ContainsKey(NewID))
        {
            PlayerManager.Instance.ConnectedPlayers.Add(NewID, this);
        }
        else
        {
            NetworkPlayer RemovedPlayer = PlayerManager.Instance.ConnectedPlayers[NewID];
            PlayerManager.Instance.ConnectedPlayers.Remove(NewID);
            PlayerManager.Instance.ConnectedPlayers.Add(NewID, this);

            Debug.Log($"Player {RemovedPlayer.name}#{RemovedPlayer.GetInstanceID()} replaced with {UserName}#{RemovedPlayer.GetInstanceID()}");
        }
    }

    private void PlayerNameChanged(string PrevName, string NewName, bool asServer)
    {
        if (PrevName == NewName)
        {
            return;
        }

        if (IsOwner)
        {
            name = "Local ";
        }
        else
        {
            name = "Connecting ";
        }
        name += $"Player {PlayerID}: ({NewName})";
        if (PlayerID == 1)
        {
            name += $" (Host)";
        }

        // Update ui??
    }

    [ServerRpc]
    public void UpdateUsername(string Username)
    {
        UserName = Username;
    }

    #endregion

    #endregion
}
