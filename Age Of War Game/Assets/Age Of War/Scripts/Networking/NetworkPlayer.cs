using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public ushort PlayerID = 0;
    public bool IsLocalPlayer = false;
    public bool ReadyForNextTurn = false;
    public bool ReadyToStart = false;
    public string UserName = "Guest";

    private void OnDestroy()
    {
        PlayerManager.Instance.ConnectedPlayers.Remove(PlayerID);
    }

    internal static void Spawn(ushort id, string username, Vector3 position, bool shouldSendSpawn = false)
    {
        NetworkPlayer player;
        if (id == NetworkManager.Instance.Client.Id)
        {
            player = Instantiate(NetworkManager.Instance.LocalPlayerPrefab, position, Quaternion.identity);
            PlayerManager.Instance.LocalPlayer = player;
            player.IsLocalPlayer = true;
        }
        else
        {
            player = Instantiate(NetworkManager.Instance.PlayerPrefab, position, Quaternion.identity);
            player.IsLocalPlayer = false;
        }
        player.transform.SetParent(NetworkManager.Instance.transform);

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
        if (NetworkManager.Instance.IsHost)
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

    private void SendSpawn()
    {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.SpawnPlayer, shouldAutoRelay: true);
        message.AddUShort(PlayerID);
        message.AddString(UserName);
        message.AddVector3(transform.position);
        NetworkManager.Instance.Client.Send(message);
    }

    internal void SendSpawn(ushort newPlayerId)
    {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.SpawnPlayer);
        message.AddUShort(newPlayerId);
        message.AddUShort(PlayerID);
        message.AddString(UserName);
        message.AddVector3(transform.position);
        NetworkManager.Instance.Client.Send(message);
    }

    [MessageHandler((ushort)MessageId.SpawnPlayer)]
    private static void SpawnPlayer(ushort fromClientId, Message message)
    {
        ushort newPlayerId = message.GetUShort();
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.SpawnPlayer);
        messageToSend.AddUShort(message.GetUShort());
        messageToSend.AddString(message.GetString());
        messageToSend.AddVector3(message.GetVector3());
        NetworkManager.Instance.Server.Send(messageToSend, newPlayerId);
    }

    [MessageHandler((ushort)MessageId.SpawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }
}
