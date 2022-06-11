using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Net;

namespace AOW.DarkRift2
{
    // Riptide Network Manager
    public class NetworkManagerDarkRift : MonoBehaviour
    {
        [SerializeField]
        private UnityClient Client;
        [SerializeField]
        private int KeepAveragePingOverSeconds = 10;
        public static NetworkManagerDarkRift Instance = null;

        [Header("Prefabs")]
        [SerializeField] private NetworkPlayer playerPrefab;
        [SerializeField] private NetworkPlayer localPlayerPrefab;

        public bool IsHost { get; set; }
        public NetworkPlayer PlayerPrefab => playerPrefab;
        public NetworkPlayer LocalPlayerPrefab => localPlayerPrefab;

        public long HostSystemTimeDifference { get; set; }
        public long LastPing { get; set; }
        public int LastPingMS { get; set; }
        public int AveragePingMS { get; set; } // Average ping over last ten seconds
        public Queue<int> AveragePingQueue { get; set; }
        public int AveragePingTally { get; set; }
        private long PingStartTime_ns = 0;
        private float PingTimer = 0;
        private int AveragePingNumber;
        public UnityClient DarkRiftClient { get { return Client; } }

        private void Awake()
        {
            HostSystemTimeDifference = -1;
            if (Instance == null)
            {
                Instance = this;
            }
            AveragePingTally = 0;
            AveragePingNumber = KeepAveragePingOverSeconds;
            AveragePingQueue = new Queue<int>(AveragePingNumber);
            Client.MessageReceived += Client_MessageReceived;
            Client.Disconnected += Client_Disconnected;
        }

        private void Update()
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                PingTimer += Time.deltaTime;
                if (PingTimer >= 1)
                {
                    PingTimer = 0;
                    PingHost();
                }
            }
        }

        public void StartHost()
        {
            IsHost = true;
            HostSystemTimeDifference = 0;
            Client.Connect(Client.Host, Client.Port, false);
        }

        public void JoinGame(string ipString)
        {
            Client.Connect(ipString, Client.Port, false);
        }

        public void LeaveGame()
        {
            HostSystemTimeDifference = -1;
            Client.Disconnect();
        }

        public void PingHost(bool ReliableSend = false)
        {
            PingStartTime_ns = System.DateTime.Now.Ticks;
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(PlayerManager.Instance.LocalPlayer.PlayerID);
                using (DarkRift.Message message = (DarkRift.Message.Create(DarkRiftTags.Ping, writer)))
                {
                    PlayerManager.Instance.DarkriftManager.DarkRiftClient.SendMessage(message, ReliableSend ? SendMode.Reliable : SendMode.Unreliable);
                }
            }
        }

        void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                //Spawn or despawn the player as necessary.
                if (message.Tag == DarkRiftTags.SpawnPlayer)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        SpawnPlayer(reader);
                    }
                }
                else if (message.Tag == DarkRiftTags.DespawnSplayer)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        DespawnPlayer(reader);
                    }
                }
                else if (message.Tag == DarkRiftTags.Ping)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        PingReturn(reader);
                    }
                }
            }
        }

        void Client_Disconnected(object sender, DisconnectedEventArgs e)
        {
            //If we disconnect then we need to destroy everything!
            foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
            {
                Destroy(player.gameObject);
            }
        }

        void SpawnPlayer(DarkRiftReader reader)
        {
            //Extract the positions
            Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            //Extract their ID
            ushort id = reader.ReadUInt16();

            //If it's a player for us then spawn us our prefab and set it up
            if (id == Client.ID)
            {
                    NetworkPlayer Player = Instantiate(
                    LocalPlayerPrefab,
                    position,
                    Quaternion.Euler(rotation)
                );

                id++; // Make sure all network types are on the same player ID for the rest of the game code
                Player.IsLocalPlayer = true;
                //Player.SetPlayerID(id);
            }
            //If it's for another player then spawn a network player and and to the manager. 
            else
            {
                    NetworkPlayer Player = Instantiate(
                    PlayerPrefab,
                    position,
                    Quaternion.Euler(rotation)
                );

                id++;
                //Player.SetPlayerID(id);
            }
        }

        void DespawnPlayer(DarkRiftReader reader)
        {
            int DespawnPlayerID = reader.ReadUInt16();
            foreach(NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
            {
                if (player.PlayerID == DespawnPlayerID)
                {
                    Destroy(player.gameObject);
                    break;
                }
            }
        }

        void PingReturn(DarkRiftReader reader)
        {
            int DespawnPlayerID = reader.ReadUInt16();
            if (PlayerManager.Instance.LocalPlayer.PlayerID == DespawnPlayerID)
            {
                // Time Difference from ping to ping back
                long Client100NanoSec = System.DateTime.Now.Ticks;
                LastPing = Client100NanoSec - PingStartTime_ns;
                LastPingMS = Mathf.RoundToInt((float)LastPing / 10000); // One MS per 10000 (100 - Nanoseconds)

                AveragePingTally += LastPingMS;
                int RemovedValue = 0;
                if (AveragePingQueue.Count == AveragePingNumber)
                {
                    RemovedValue = Instance.AveragePingQueue.Dequeue();
                }

                AveragePingTally -= RemovedValue;
                AveragePingQueue.Enqueue(LastPingMS);
                AveragePingMS = AveragePingTally / AveragePingNumber;
            }
        }
    }

}
static class DarkRiftTags
{
    public static readonly ushort SpawnPlayer = 0;
    public static readonly ushort DespawnSplayer = 1;
    public static readonly ushort UpdatePlayerName = 2;
    public static readonly ushort Ping = 3;
    public static readonly ushort PlayerReady = 4;
    public static readonly ushort SendCustomGamePagePlayerData = 5;
}