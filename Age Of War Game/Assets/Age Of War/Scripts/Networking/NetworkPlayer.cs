using FishNet.Object;
using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AOW.RiptideNetworking;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using DarkRift.Client;
using DarkRift;

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
        if (PlayerManager.Instance.NetworkType == NetworkingTypes.Fishynet)
        {
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
        else if (PlayerManager.Instance.NetworkType == NetworkingTypes.Riptide)
        {
            if (!PlayerManager.Instance.ConnectedPlayers.ContainsKey(PlayerID))
            {
                PlayerManager.Instance.ConnectedPlayers.Add(PlayerID, this);
            }
        }
        else if (PlayerManager.Instance.NetworkType == NetworkingTypes.Darkrift2)
        {
            PlayerManager.Instance.DarkriftManager.DarkRiftClient.MessageReceived += MessageRecieved;
            if (IsLocalPlayer)
            {
                UserName = PlayerManager.Instance.LocalPlayerData.Data.UserName;
                PlayerManager.Instance.LocalPlayer = this;
                SendChangeNameMessage();
            }
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

    #region Darkrift

    public void SetPlayerID(ushort NewID)
    {
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

    public void SendChangeNameMessage()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(PlayerID);
            writer.Write(UserName);
            using (DarkRift.Message message = (DarkRift.Message.Create(DarkRiftTags.UpdatePlayerName, writer)))
            {
                PlayerManager.Instance.DarkriftManager.DarkRiftClient.SendMessage(message, SendMode.Reliable);
            }
        }
    }

    public void MessageRecieved(object sender, MessageReceivedEventArgs e)
    {
        using (DarkRift.Message message = e.GetMessage() as DarkRift.Message)
        {
            //Spawn or despawn the player as necessary.
            if (message.Tag == DarkRiftTags.UpdatePlayerName)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    RecieveChangeNameMessage(reader);
                }
            }
        }
    }

    public void RecieveChangeNameMessage(DarkRiftReader reader)
    {
        ushort ChangedPlayerID = reader.ReadUInt16();
        if (ChangedPlayerID == PlayerID)
        {
            string UserName = reader.ReadString();
            if (IsLocalPlayer)
            {
                gameObject.name = $"Local Player {PlayerID}: ({UserName})";
            }
            else
            {
                gameObject.name = $"Connecting Player {PlayerID}: ({UserName})";
            }

            if (PlayerID == 1)
            {
                gameObject.name += " (Host)";
            }
        }
    }

    #endregion

    #endregion

    #region Spawn RPC
    internal static void Spawn(ushort id, string username, Vector3 position, bool shouldSendSpawn = false)
    {
        NetworkPlayer player;
        if (PlayerManager.Instance.NetworkType == NetworkingTypes.Riptide)
        {
            if (id == AOW.RiptideNetworking.NetworkManager.Instance.Client.Id)
            {
                player = Instantiate(AOW.RiptideNetworking.NetworkManager.Instance.LocalPlayerPrefab, position, Quaternion.identity);
                PlayerManager.Instance.LocalPlayer = player;
                player.IsLocalPlayer = true;
            }
            else
            {
                player = Instantiate(AOW.RiptideNetworking.NetworkManager.Instance.PlayerPrefab, position, Quaternion.identity);
                player.IsLocalPlayer = false;
            }
            player.transform.SetParent(AOW.RiptideNetworking.NetworkManager.Instance.transform);

            player.PlayerID = id;
            player.UserName = username;

            if (player.IsLocalPlayer)
            {
                player.name = "Local ";
            }
            else
            {
                player.name = "Connecting ";
            }
            player.name += $"Player {id}: ({username})";
            if (AOW.RiptideNetworking.NetworkManager.Instance.IsHost)
            {
                player.name += $" (Host)";
            }

            PlayerManager.Instance.ConnectedPlayers.Add(id, player);
            if (shouldSendSpawn)
            {
                player.SendSpawn();
            }

            if (!player.IsLocalPlayer)
            {
                PlayerManager.Instance.OtherPlayerJoined();
            }
        }
    }

    private void SendSpawn()
    {
        if (PlayerManager.Instance.NetworkType == NetworkingTypes.Riptide)
        {
            RiptideNetworking.Message message = RiptideNetworking.Message.Create(MessageSendMode.reliable, MessageId.SpawnPlayer, shouldAutoRelay: true);
            message.AddUShort(PlayerID);
            message.AddString(UserName);
            message.AddVector3(transform.position);
            AOW.RiptideNetworking.NetworkManager.Instance.Client.Send(message);
        }
    }

    internal void SendSpawn(ushort newPlayerId)
    {
        if (PlayerManager.Instance.NetworkType == NetworkingTypes.Riptide)
        {
            RiptideNetworking.Message message = RiptideNetworking.Message.Create(MessageSendMode.reliable, MessageId.SpawnPlayer);
            message.AddUShort(newPlayerId);
            message.AddUShort(PlayerID);
            message.AddString(UserName);
            message.AddVector3(transform.position);
            AOW.RiptideNetworking.NetworkManager.Instance.Client.Send(message);
        }
    }

    #region Riptide

    [MessageHandler((ushort)MessageId.SpawnPlayer)]
    private static void SpawnPlayer(ushort fromClientId, RiptideNetworking.Message message)
    {
        ushort newPlayerId = message.GetUShort();
        RiptideNetworking.Message messageToSend = RiptideNetworking.Message.Create(MessageSendMode.reliable, MessageId.SpawnPlayer);
        messageToSend.AddUShort(message.GetUShort());
        messageToSend.AddString(message.GetString());
        messageToSend.AddVector3(message.GetVector3());
        AOW.RiptideNetworking.NetworkManager.Instance.Server.Send(messageToSend, newPlayerId);
    }

    [MessageHandler((ushort)MessageId.SpawnPlayer)]
    private static void SpawnPlayer(RiptideNetworking.Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }
    #endregion

    #endregion
}
