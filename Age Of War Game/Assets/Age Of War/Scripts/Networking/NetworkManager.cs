using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance = null;

    [SerializeField] private ushort port;
    [Header("Prefabs")]
    [SerializeField] private NetworkPlayer playerPrefab;
    [SerializeField] private NetworkPlayer localPlayerPrefab;

    internal Server Server { get; private set; }
    internal Client Client { get; private set; }

    public bool IsHost { get; set; }
    public NetworkPlayer PlayerPrefab => playerPrefab;
    public NetworkPlayer LocalPlayerPrefab => localPlayerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server { AllowAutoMessageRelay = true };

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientConnected += PlayerJoined;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
    }

    private void FixedUpdate()
    {
        if (Server.IsRunning)
        {
            IsHost = true;
            Server.Tick();
        }
        else if (IsHost)
        {
            IsHost = false;
        }

        Client.Tick();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
        DisconnectClient();
    }

    internal void StartHost()
    {
        Server.Start(port, 2);
        Client.Connect($"127.0.0.1:{port}");
    }

    internal void JoinGame(string ipString)
    {
        Server.Stop();
        Client.Connect($"{ipString}:{port}");
    }

    internal void LeaveGame()
    {
        Server.Stop();
        DisconnectClient();
    }

    private void DisconnectClient()
    {
        Client.Disconnect();
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            Destroy(player.gameObject);
        }
    }

    private void DidConnect(object sender, EventArgs e)
    {
        NetworkPlayer.Spawn(Client.Id, PlayerManager.Instance.LocalPlayerData.Data.UserName, Vector3.zero, true);
    }

    private void FailedToConnect(object sender, EventArgs e)
    {

    }

    private void PlayerJoined(object sender, ClientConnectedEventArgs e)
    {
        PlayerManager.Instance.ConnectedPlayers[Client.Id].SendSpawn(e.Id);
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(PlayerManager.Instance.ConnectedPlayers[e.Id].gameObject);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            Destroy(player.gameObject);
        }
    }
}

internal enum MessageId : ushort
{
    ChangeScene = 0,
    SpawnPlayer = 1,

    // Lockstep - Send Actions and Initialization
    TurnConfirmation = 2,
    SendTurnActions = 3,
    SendPlayerReadyForSimulation = 4,

    // Main Menu
    //Online
    SendPlayerData = 5,
    // Custom Game
    ReadyButtonPressed = 6,
    SendCustomGameRacePerkData = 7,
}
