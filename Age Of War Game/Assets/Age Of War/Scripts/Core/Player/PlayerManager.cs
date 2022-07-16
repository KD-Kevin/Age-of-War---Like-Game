using BitStrap;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Transporting;
using HeathenEngineering.SteamworksIntegration;
using AgeOfWar.Data;
using AgeOfWar.UI;
using AgeOfWar.Networking;
using AgeOfWar.Core.Units;
using AgeOfWar.AI;

namespace AgeOfWar.Core
{
    public class PlayerManager : MonoBehaviour
    {
        public FishNet.Managing.NetworkManager FishnetNetworkManager;
        public LobbyManager SteamLobbyManager;
        public FishnetNetworkHelper NetworkHelperPrefab;
        public FishnetNetworkHelper NetworkHelper { get; set; }

        public static PlayerManager Instance = null;
        public PlayModes ActiveOnlineMode = PlayModes.None;

        // Use to store data for player and opponent
        public PlayerDataScriptableObject LocalPlayerData;
        public PlayerDataScriptableObject CustomGameOpponentData;
        public PlayerDataScriptableObject QuickplayOpponentData;
        public PlayerDataScriptableObject RankedOpponentData;

        public MatchData CurrentMatchData = new MatchData();
        public List<(PlayerData, PlayModes)> PreviousPlayerData = new List<(PlayerData, PlayModes)>(); // Keeps track of your previous players you played against and in which modes
        public List<MatchData> PreviousMatchData = new List<MatchData>();

        public AiController OpponentAiController;
        // Ai Difficulties - Defaults if race does ot have Ai Data
        public GameAiScriptableObject DefaultVeryEasyAi;
        public GameAiScriptableObject DefaultEasyAi;
        public GameAiScriptableObject DefaultNormalAi;
        public GameAiScriptableObject DefaultHardAi;
        public GameAiScriptableObject DefaultInsaneAi;
        public GameAiScriptableObject DefaultYouWontAi;

        // Campaign
        public CampaignSaveData CurrentCampaignSaveData { get; set; }

        // Online
        public NetworkPlayer LocalPlayer { get; set; }
        public Dictionary<ushort, NetworkPlayer> ConnectedPlayers = new Dictionary<ushort, NetworkPlayer>();

        // Offline
        public bool OfflineGameStarted { get; set; }

        // Custom Game
        private System.Action<PlayerData> ConfirmationCustomGameOpponentCallback = null;
        private System.Action CancellationCustomGameOpponentCallback = null;

        // Quickplay Game
        private System.Action<PlayerData> ConfirmationQuickplayOpponentCallback = null;
        private System.Action CancellationQuickplayOpponentCallback = null;

        // Ranked Game
        private System.Action<PlayerData> ConfirmationRankedOpponentCallback = null;
        private System.Action CancellationRankedCallback = null;

        public LocalConnectionStates ClientState { get; set; }
        public LocalConnectionStates ServerState { get; set; }


        void Awake()
        {
            ClientState = LocalConnectionStates.Stopped;
            ServerState = LocalConnectionStates.Stopped;

            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            // Load Player Data
            string SavedUserName = PlayerPrefs.GetString("SaveName", "No Name");
            if (SavedUserName != "No Name")
            {
                PlayerData LoadedData = PlayerData.LoadLocal(SavedUserName);
                if (LoadedData != null)
                {
                    LocalPlayerData.Data = LoadedData;
                }
            }

            // Check Steam Info
            try
            {
                string SteamUsername = UserData.Me.Nickname;
                LocalPlayerData.Data.UserName = SteamUsername;
                if (SavedUserName != SteamUsername)
                {
                    PlayerPrefs.SetString("SaveName", SteamUsername);
                    LocalPlayerData.Data.SaveLocal(SteamUsername);
                }
                Debug.Log($"Loaded Steam Username {SteamUsername}");
            }
            catch
            {
                Debug.Log("Could not Get Steam Username");
            }
        }

        private void Start()
        {
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerStateChange;
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionStateChange;
            SceneLoadManager.Instance.InitializeBroadcasts();
        }

        private void OnDestroy()
        {
            if (LocalPlayerData.Data.UserName != "No Name" && !string.IsNullOrEmpty(LocalPlayerData.Data.UserName))
            {
                PlayerPrefs.SetString("SaveName", LocalPlayerData.Data.UserName);
                LocalPlayerData.Data.SaveLocal(LocalPlayerData.Data.UserName);
            }

            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerStateChange;
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionStateChange;
        }

        #region Online
        // Online
        public bool WaitingOnSpawn { get; set; }
        public void SpawnFishnetNetworkHelper()
        {
            if (ServerState == LocalConnectionStates.Started)
            {
                WaitingOnSpawn = false;
                if (NetworkHelper == null)
                {
                    FishnetNetworkHelper Helper = Instantiate(NetworkHelperPrefab);
                    NetworkHelper = Helper;
                    InstanceFinder.ServerManager.Spawn(Helper.gameObject, null);
                }
            }
            else if (ServerState == LocalConnectionStates.Starting && !WaitingOnSpawn)
            {
                WaitingOnSpawn = true;
                Invoke(nameof(SpawnFishnetNetworkHelper), 0.1f);
            }
        }

        #region Ready To Start RPC

        public void SendReadyToStart()
        {
            if (ActiveOnlineMode == PlayModes.CustomGame || ActiveOnlineMode == PlayModes.Quickplay || ActiveOnlineMode == PlayModes.Ranked)
            {
                NetworkHelper.SendReady(LocalPlayer);
            }
            else
            {
                OfflineGameStarted = true;
                GameAiScriptableObject AiData;
                if (CurrentMatchData.AiDifficulty == CampaignDifficulty.Normal)
                {
                    if (CurrentMatchData.OpponentSelectedRace.NormalAi == null)
                    {
                        AiData = DefaultNormalAi;
                    }
                    else
                    {
                        AiData = CurrentMatchData.OpponentSelectedRace.NormalAi;
                    }
                }
                else if (CurrentMatchData.AiDifficulty == CampaignDifficulty.Hard)
                {
                    if (CurrentMatchData.OpponentSelectedRace.HardAi == null)
                    {
                        AiData = DefaultHardAi;
                    }
                    else
                    {
                        AiData = CurrentMatchData.OpponentSelectedRace.HardAi;
                    }
                }
                else if (CurrentMatchData.AiDifficulty == CampaignDifficulty.Easy)
                {
                    if (CurrentMatchData.OpponentSelectedRace.EasyAi == null)
                    {
                        AiData = DefaultVeryEasyAi;
                    }
                    else
                    {
                        AiData = CurrentMatchData.OpponentSelectedRace.EasyAi;
                    }
                }
                else if (CurrentMatchData.AiDifficulty == CampaignDifficulty.Insane)
                {
                    if (CurrentMatchData.OpponentSelectedRace.InsaneAi == null)
                    {
                        AiData = DefaultInsaneAi;
                    }
                    else
                    {
                        AiData = CurrentMatchData.OpponentSelectedRace.InsaneAi;
                    }
                }
                else if (CurrentMatchData.AiDifficulty == CampaignDifficulty.YouWont)
                {
                    if (CurrentMatchData.OpponentSelectedRace.YouWontAi == null)
                    {
                        AiData = DefaultYouWontAi;
                    }
                    else
                    {
                        AiData = CurrentMatchData.OpponentSelectedRace.YouWontAi;
                    }
                }
                else
                {
                    if (CurrentMatchData.OpponentSelectedRace.VeryEasyAi == null)
                    {
                        AiData = DefaultVeryEasyAi;
                    }
                    else
                    {
                        AiData = CurrentMatchData.OpponentSelectedRace.VeryEasyAi;
                    }
                }

                // Load the opponent Ai
                OpponentAiController.LoadController(CurrentMatchData.OpponentSelectedRace, AiData, 2);
            }
        }

        #endregion

        public bool EveryoneIsReadyForStart()
        {
            if (ConnectedPlayers.Count <= 1)
            {
                return false;
            }

            foreach (NetworkPlayer player in ConnectedPlayers.Values)
            {
                if (!player.ReadyToStart)
                {
                    return false;
                }
            }

            return true;
        }

        #region Player Joined

        public void OtherPlayerJoined()
        {
            if (ActiveOnlineMode == PlayModes.Ranked || ActiveOnlineMode == PlayModes.Quickplay || ActiveOnlineMode == PlayModes.CustomGame)
            {
                Debug.Log("Other Player Joined");
            }
            else
            {
                Debug.LogWarning("Non online mode - should not be able to connect to other players");
            }
        }

        public void OnServerStateChange(ServerConnectionStateArgs args)
        {
            ServerState = args.ConnectionState;
        }

        public void OnClientConnectionStateChange(ClientConnectionStateArgs args)
        {
            ClientState = args.ConnectionState;
        }

        public void OtherNetworkPlayerConnected(NetworkPlayer Player)
        {
            //Debug.Log(NetworkHelper == null ? "Network Helper Null": LocalPlayerData == null ? "Local Player Data Null" : LocalPlayerData.Data == null ? " Local Player Save Data Null" : LocalPlayerData.Data.SerializeToJSON() == null ? "JSON Save Data is Null" : "Send Player Data");
            NetworkHelper.SendDataToNetworkPlayer(Player, LocalPlayerData.Data.SerializeToJSON());
        }

        public void RecievePlayerData(PlayerData OtherPlayersData)
        {
            if (Instance.ActiveOnlineMode == PlayModes.Ranked)
            {
                Instance.ConfirmationRankedOpponentCallback(OtherPlayersData);
            }
            else if (Instance.ActiveOnlineMode == PlayModes.Quickplay)
            {
                Instance.ConfirmationQuickplayOpponentCallback(OtherPlayersData);
            }
            else if (Instance.ActiveOnlineMode == PlayModes.CustomGame)
            {
                //Debug.Log("Send Player Data 5");
                Instance.ConfirmationCustomGameOpponentCallback(OtherPlayersData);
            }
        }

        #endregion


        public BaseUnitBehaviour GetUnitByIndex(int unitBuyIndex, ushort owningPlayer)
        {
            if (CurrentMatchData.PlayMode == PlayModes.CustomGame || CurrentMatchData.PlayMode == PlayModes.Ranked || CurrentMatchData.PlayMode == PlayModes.Quickplay)
            {
                if (owningPlayer == LocalPlayer.PlayerID)
                {
                    return CurrentMatchData.PlayerSelectedRace.StartingUnitsBlueprints[unitBuyIndex];
                }
                else
                {
                    return CurrentMatchData.OpponentSelectedRace.StartingUnitsBlueprints[unitBuyIndex];
                }
            }
            else
            {
                // Player ID may not be the way it should work in local run games
                if (owningPlayer == 1)
                {
                    return CurrentMatchData.PlayerSelectedRace.StartingUnitsBlueprints[unitBuyIndex];
                }
                else
                {
                    return CurrentMatchData.OpponentSelectedRace.StartingUnitsBlueprints[unitBuyIndex];
                }
            }
        }

        // Steam
        public void OnRoomCreated(Lobby RoomCreated)
        {
            if (ActiveOnlineMode == PlayModes.CustomGame)
            {
                CustomGamePage.Instance.OnRoomCreated(RoomCreated);
            }
        }

        public void OnRoomJoined(Lobby Room)
        {
            if (ActiveOnlineMode == PlayModes.CustomGame)
            {
                CustomGamePage.Instance.OnRoomJoined(Room);
            }

            FishnetNetworkManager.ClientManager.StartConnection(Room.Owner.user.cSteamId.m_SteamID.ToString());
        }

        public void OnRoomNotCreated()
        {
            if (ActiveOnlineMode == PlayModes.CustomGame)
            {
                CustomGamePage.Instance.OnRoomNotCreated();
            }
        }

        // Quickplay
        public void RequestOpponentQuickplay(System.Action<PlayerData> FindPlayer, System.Action CannotFindPlayer)
        {
            ActiveOnlineMode = PlayModes.Quickplay;
            ConfirmationQuickplayOpponentCallback = FindPlayer;
            CancellationQuickplayOpponentCallback = CannotFindPlayer;
        }

        public void FoundQuickplayOpponent(PlayerData Player)
        {
            QuickplayOpponentData.Data = Player;
            ConfirmationQuickplayOpponentCallback.Invoke(Player);
        }

        public void CancelFindQuickplayOpponent()
        {
            CancellationQuickplayOpponentCallback.Invoke();
        }


        // Custom Game
        public void RequestOpponentCustomGame(System.Action<PlayerData> FindPlayer, System.Action CannotFindPlayer)
        {
            ActiveOnlineMode = PlayModes.CustomGame;
            ConfirmationCustomGameOpponentCallback = FindPlayer;
            CancellationCustomGameOpponentCallback = CannotFindPlayer;
        }

        public void RequestOpponentCustomGameDirectConnect(System.Action<PlayerData> FindPlayer, System.Action CannotFindPlayer, string directConnectIp)
        {
            ActiveOnlineMode = PlayModes.CustomGame;
            ConfirmationCustomGameOpponentCallback = FindPlayer;
            CancellationCustomGameOpponentCallback = CannotFindPlayer;

            SteamLobbyManager.Join(directConnectIp);
        }

        public void FoundCustomGameOpponent(PlayerData Player)
        {
            CustomGameOpponentData.Data = Player;
            ConfirmationCustomGameOpponentCallback.Invoke(Player);
        }

        public void CancelFindCustomGameOpponent()
        {
            ActiveOnlineMode = PlayModes.None;
            CancellationCustomGameOpponentCallback.Invoke();
        }

        public void SetMatchData(CustomGamePage CGP, bool MakeNewData = false)
        {
            if (MakeNewData)
            {
                CurrentMatchData = new MatchData();
            }

            CurrentMatchData.PlayMode = PlayModes.CustomGame;

            CurrentMatchData.PlayersData = LocalPlayerData.Data;
            CurrentMatchData.PlayerSelectedRace = CGP.PlayerSelectedRace;
            CurrentMatchData.PlayerSelectedPerk1 = CGP.PlayerSelectedPerk1;
            CurrentMatchData.PlayerSelectedPerk2 = CGP.PlayerSelectedPerk2;

            CurrentMatchData.OpponentsData = CGP.OpponentPlayerData;
            CurrentMatchData.OpponentSelectedRace = CGP.OpponentSelectedRace;
            CurrentMatchData.OpponentSelectedPerk1 = CGP.OpponentSelectedPerk1;
            CurrentMatchData.OpponentSelectedPerk2 = CGP.OpponentSelectedPerk2;
        }

        // Ranked Game
        public void RequestOpponentRanked(System.Action<PlayerData> FindPlayer, System.Action CannotFindPlayer)
        {
            ActiveOnlineMode = PlayModes.Ranked;
            ConfirmationRankedOpponentCallback = FindPlayer;
            CancellationRankedCallback = CannotFindPlayer;
        }

        public void FoundRankedOpponent(PlayerData Player)
        {
            RankedOpponentData.Data = Player;
            ConfirmationRankedOpponentCallback.Invoke(Player);
        }

        public void CancelFindRankedOpponent()
        {
            CancellationRankedCallback.Invoke();
        }

        #endregion

        #region Offline

        // Load Page Data
        public void LoadPageData(VersusAiPage page)
        {
            CurrentMatchData.PlayMode = PlayModes.VsComputer;
            CurrentMatchData.AiDifficulty = page.CurrentDifficulty;

            CurrentMatchData.PlayersData = LocalPlayerData.Data;
            CurrentMatchData.PlayerSelectedRace = page.PlayerSelectedRace;
            CurrentMatchData.PlayerSelectedPerk1 = page.PlayerSelectedPerk1;
            CurrentMatchData.PlayerSelectedPerk2 = page.PlayerSelectedPerk2;

            CurrentMatchData.OpponentsData = null;
            CurrentMatchData.OpponentSelectedRace = page.AiSelectedRace;
            CurrentMatchData.OpponentSelectedPerk1 = page.AiSelectedPerk1;
            CurrentMatchData.OpponentSelectedPerk2 = page.AiSelectedPerk2;
        }

        public void LoadPageData(CampaignPage page)
        {
            CurrentCampaignSaveData = page.CurrentSaveData;
        }

        public void StartMatch()
        {
            if (CurrentMatchData.PlayMode == PlayModes.None)
            {
                return;
            }

            SceneLoadManager.Instance.LoadScene("Game");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += LoadGameScene;
        }

        public void StartCampaign()
        {
            if (CurrentCampaignSaveData == null || CurrentCampaignSaveData.CampainRace == null)
            {
                return;
            }

            SceneLoadManager.Instance.LoadScene(CurrentCampaignSaveData.CampainRace.CampainSceneName);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += LoadCampaignScene;
        }

        public void LoadGameScene(Scene loadedScene, LoadSceneMode Mode)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= LoadGameScene;
        }

        public void LoadCampaignScene(Scene loadedScene, LoadSceneMode Mode)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= LoadCampaignScene;
        }

        [Button]
        public void ClearNameSaveData()
        {
            LocalPlayerData.Data.UserName = "No Name";
            PlayerPrefs.SetString("SaveName", "No Name");
        }

        public void AddAi(int TeamID)
        {

        }

        #endregion
    }

    [System.Serializable]
    public class MatchData
    {
        public PlayModes PlayMode = PlayModes.None;

        public CampaignDifficulty AiDifficulty = CampaignDifficulty.Normal;

        public PlayerData PlayersData = null;
        public RaceDataScriptableObject PlayerSelectedRace = null;
        public Perk PlayerSelectedPerk1 = null;
        public Perk PlayerSelectedPerk2 = null;

        public PlayerData OpponentsData = null; // Stays Null for Ai based opponents
        public RaceDataScriptableObject OpponentSelectedRace = null;
        public Perk OpponentSelectedPerk1 = null;
        public Perk OpponentSelectedPerk2 = null;

        public MatchData Clone()
        {
            MatchData clone = new MatchData();

            clone.PlayMode = PlayMode;
            clone.AiDifficulty = AiDifficulty;

            clone.PlayersData = PlayersData;
            clone.PlayerSelectedRace = PlayerSelectedRace;
            clone.PlayerSelectedPerk1 = PlayerSelectedPerk1;
            clone.PlayerSelectedPerk2 = PlayerSelectedPerk2;

            clone.OpponentsData = OpponentsData;
            clone.OpponentSelectedRace = OpponentSelectedRace;
            clone.OpponentSelectedPerk1 = OpponentSelectedPerk1;
            clone.OpponentSelectedPerk2 = OpponentSelectedPerk2;

            return clone;
        }
    }
}
