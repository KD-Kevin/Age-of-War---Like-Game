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

    public long HostSystemTimeDifference { get; set; }
    public long LastPing { get; set; }
    public int LastPingMS { get; set; }
    private long PingStartTime_ns = 0;

    private void Awake()
    {
        HostSystemTimeDifference = -1;
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
        IsHost = true;
        HostSystemTimeDifference = 0;
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
        HostSystemTimeDifference = -1;
        Server.Stop();
        DisconnectClient();
    }

    private void DisconnectClient()
    {
        HostSystemTimeDifference = -1;
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
        HostSystemTimeDifference = -1;
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            Destroy(player.gameObject);
        }
    }

    public void PingHost()
    {
        PingStartTime_ns = System.DateTime.Now.Ticks;
        Message message = Message.Create(MessageSendMode.reliable, MessageId.PingHost);
        Client.Send(message);
    }


    [MessageHandler((ushort)MessageId.PingHost)]
    private static void PingHost(ushort fromClientId, Message message)
    {
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.PingHost);
        messageToSend.AddLong(System.DateTime.Now.Ticks);
        NetworkManager.Instance.Server.Send(messageToSend, fromClientId);
    }

    [MessageHandler((ushort)MessageId.PingHost)]
    private static void PingHost(Message message)
    {
        long Host100NanoSec = message.GetLong();

        // Time Difference from ping to ping back
        long Client100NanoSec = System.DateTime.Now.Ticks;
        Instance.LastPing = Client100NanoSec - Instance.PingStartTime_ns;
        Instance.LastPingMS = Mathf.RoundToInt((float)Instance.LastPing / 10000); // One MS per 10000 (100 - Nanoseconds)

        if (Instance.HostSystemTimeDifference == -1)
        {
            //                            (         Estimate Time when Ping was             )
            long EstimatedHostTime = Host100NanoSec - Instance.LastPing / 2;
            Instance.HostSystemTimeDifference = Instance.PingStartTime_ns - EstimatedHostTime;
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

    // Countdown
    SendStartCoundDownSec = 8,
    PingHost = 9,
}
