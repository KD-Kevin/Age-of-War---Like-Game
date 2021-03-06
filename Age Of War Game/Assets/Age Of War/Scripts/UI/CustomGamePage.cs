using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FishNet;
using FishNet.Transporting;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Object;
using DarkRift;
using HeathenEngineering.SteamworksIntegration;
using AgeOfWar.Data;
using AgeOfWar.Core;
using AgeOfWar.Networking;

namespace AgeOfWar.UI
{
    public class CustomGamePage : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI PlayerNameText;

        [SerializeField]
        private TextMeshProUGUI PlayerRaceText;
        [SerializeField]
        private Image PlayerRaceImage;

        [SerializeField]
        private Sprite NoPerkSprite;
        [SerializeField]
        private Sprite NoRaceSprite;
        [SerializeField]
        private TextMeshProUGUI PlayerPerk1Text;
        [SerializeField]
        private Image PlayerPerk1Image;
        [SerializeField]
        private Button PlayerPerk1Button;
        [SerializeField]
        private TextMeshProUGUI PlayerPerk2Text;
        [SerializeField]
        private Image PlayerPerk2Image;
        [SerializeField]
        private Button PlayerPerk2Button;

        [SerializeField]
        private TextMeshProUGUI OpponentNameText;

        [SerializeField]
        private TextMeshProUGUI OpponentRaceText;
        [SerializeField]
        private Image OpponentRaceImage;

        [SerializeField]
        private TextMeshProUGUI OpponentPerk1Text;
        [SerializeField]
        private Image OpponentPerk1Image;
        [SerializeField]
        private Button OpponentPerk1Button;
        [SerializeField]
        private TextMeshProUGUI OpponentPerk2Text;
        [SerializeField]
        private Image OpponentPerk2Image;
        [SerializeField]
        private Button OpponentPerk2Button;
        [SerializeField]
        private Button OpponentRaceButton;

        [SerializeField]
        private GameObject[] OpponentUi;
        [SerializeField]
        private GameObject[] SearchingUi;
        [SerializeField]
        private GameObject[] NotSearchingUi;

        [SerializeField]
        private Button PlayButton;
        [SerializeField]
        private Button ReadyButton;
        [SerializeField]
        private GameObject PlayerReadyObject;
        [SerializeField]
        private GameObject OpponentReadyObject;
        [SerializeField]
        private Button HostButton;
        [SerializeField]
        private Button CancelHostButton;
        [SerializeField]
        private TextMeshProUGUI SteamLobbyRoomID;

        public RaceDataScriptableObject PlayerSelectedRace { get; set; }
        public Perk PlayerSelectedPerk1 { get; set; }
        public Perk PlayerSelectedPerk2 { get; set; }

        public RaceDataScriptableObject OpponentSelectedRace { get; set; }
        public Perk OpponentSelectedPerk1 { get; set; }
        public Perk OpponentSelectedPerk2 { get; set; }
        public PlayerData OpponentPlayerData { get; set; }

        public bool PlayerReady { get; set; }
        public bool OpponentReady { get; set; }
        public bool PlayerButtonWasPressed { get; set; }

        public MultiplayStatus MultiplayerStatus { get; set; }
        public static CustomGamePage Instance = null;
        private string DirectConnectString = "None";
        private float PingTimer = 0;
        public Lobby CurrentLobby;

        private void Awake()
        {
            PlayerButtonWasPressed = false;
            Instance = this;
            InstanceFinder.NetworkManager.ServerManager.RegisterBroadcast<PreGameDataBroadcast>(BroadCastPlayDataToServer);
            InstanceFinder.NetworkManager.ClientManager.RegisterBroadcast<PreGameDataBroadcastResponse>(BroadCastPlayDataToClients);
        }

        private void OnDisable()
        {
            if (MainMenu.Instance.MainMenuMode != MainMenuModes.FrontMenu)
            {
                MainMenu.Instance.MainMenuMode = MainMenuModes.FrontMenu;
            }
        }

        private void Update()
        {
            if (PlayerManager.Instance.ClientState == LocalConnectionStates.Started && PlayerManager.Instance.LocalPlayer != null)
            {
                PlayButton.interactable = PlayerManager.Instance.LocalPlayer.IsHost && !PlayerButtonWasPressed;
            }
            else if (PlayButton.interactable)
            {
                PlayButton.interactable = false;
            }

            if (PlayerManager.Instance.ClientState == LocalConnectionStates.Started)
            {
                PingTimer += Time.deltaTime;
                if (PingTimer > 1)
                {
                    PingTimer -= 1;
                    LockstepManager.Instance.PingHost();
                }
            }
        }

        public void OpenPage()
        {
            gameObject.SetActive(true);
            PlayerNameText.text = PlayerManager.Instance.LocalPlayerData.Data.UserName;
            OpponentRaceButton.interactable = false;
            OpponentPerk1Button.interactable = false;
            OpponentPerk2Button.interactable = false;

            PlayerSelectedRace = null;
            PlayerSelectedPerk1 = null;
            PlayerSelectedPerk2 = null;

            OpponentPlayerData = null;
            OpponentSelectedRace = null;
            OpponentSelectedPerk1 = null;
            OpponentSelectedPerk2 = null;
            SteamLobbyRoomID.text = $"Room ID: No Room";

            if (MultiplayerStatus == MultiplayStatus.Searching)
            {
                foreach (GameObject UiObject in SearchingUi)
                {
                    UiObject.SetActive(true);
                }

                foreach (GameObject UiObject in NotSearchingUi)
                {
                    UiObject.SetActive(false);
                }

                foreach (GameObject UiObject in OpponentUi)
                {
                    UiObject.SetActive(false);
                }
            }
            else if (MultiplayerStatus == MultiplayStatus.FoundMatch)
            {
                foreach (GameObject UiObject in SearchingUi)
                {
                    UiObject.SetActive(false);
                }

                foreach (GameObject UiObject in NotSearchingUi)
                {
                    UiObject.SetActive(false);
                }

                foreach (GameObject UiObject in OpponentUi)
                {
                    UiObject.SetActive(true);
                }
            }
            else
            {
                foreach (GameObject UiObject in SearchingUi)
                {
                    UiObject.SetActive(false);
                }

                foreach (GameObject UiObject in NotSearchingUi)
                {
                    UiObject.SetActive(true);
                }

                foreach (GameObject UiObject in OpponentUi)
                {
                    UiObject.SetActive(false);
                }
            }
            PlayerManager.Instance.SetMatchData(this, true);
        }

        public void OpenRaceSelectorForPlayer()
        {
            RaceSelector.Instance.OpenRaceSelector(ConfirmPlayerRaceSelection, CancelPlayerRaceSelection);
        }

        public void ConfirmPlayerRaceSelection(RaceDataScriptableObject SelectedRace)
        {
            PlayerSelectedRace = SelectedRace;
            ClearPlayerPerks();
            PlayerPerk1Button.interactable = true;
            PlayerPerk2Button.interactable = true;

            PlayerRaceImage.sprite = SelectedRace.DisplaySprite;
            PlayerRaceText.text = SelectedRace.DisplayName;

            if (PlayerSelectedRace == null)
            {
                PlayButton.interactable = false;
            }
            else
            {
                PlayButton.interactable = true;
            }

            if (PlayerManager.Instance.ConnectedPlayers.Count > 1)
            {
                SendGameData();
            }

            PlayerManager.Instance.SetMatchData(this);
        }

        public void CancelPlayerRaceSelection()
        {

        }

        public void ClearPlayerPerks()
        {
            PlayerSelectedPerk1 = null;
            PlayerSelectedPerk2 = null;

            PlayerPerk1Image.sprite = NoPerkSprite;
            PlayerPerk2Image.sprite = NoPerkSprite;

            PlayerPerk1Text.text = "No Perk Selected";
            PlayerPerk2Text.text = "No Perk Selected";
        }

        public void OpenPerkSelectorForPlayer()
        {
            if (PlayerSelectedRace == null)
            {
                return;
            }
            PerkSelector.Instance.OpenPerkSelector(PlayerSelectedRace, ConfirmPlayerPerk1Selection, CancelPlayerPerk1Selection, PlayerSelectedPerk1, PlayerSelectedPerk2);
        }

        public void ConfirmPlayerPerk1Selection(Perk SelectedPerk1, Perk SelectedPerk2)
        {
            PlayerSelectedPerk1 = SelectedPerk1;
            if (PlayerSelectedPerk1 != null)
            {
                PlayerPerk1Image.sprite = SelectedPerk1.DisplaySprite;
                PlayerPerk1Text.text = SelectedPerk1.DisplayName;
            }

            PlayerSelectedPerk2 = SelectedPerk2;
            if (PlayerSelectedPerk2 != null)
            {
                PlayerPerk2Image.sprite = SelectedPerk2.DisplaySprite;
                PlayerPerk2Text.text = SelectedPerk2.DisplayName;
            }

            if (PlayerManager.Instance.ConnectedPlayers.Count > 1)
            {
                SendGameData();
            }

            PlayerManager.Instance.SetMatchData(this);
        }

        public void CancelPlayerPerk1Selection()
        {

        }

        public void LoadOpponentPlayer(RaceDataScriptableObject Race, Perk Perk1, Perk Perk2)
        {
            ClearOpponentPerks();

            OpponentSelectedRace = Race;
            OpponentSelectedPerk1 = Perk1;
            OpponentSelectedPerk2 = Perk2;

            if (Race != null)
            {
                OpponentRaceImage.sprite = OpponentSelectedRace.DisplaySprite;
                OpponentRaceText.text = OpponentSelectedRace.DisplayName;
            }
            else
            {
                OpponentRaceImage.sprite = NoRaceSprite;
                OpponentRaceText.text = "No Race Selected";
            }

            if (OpponentSelectedPerk1 != null)
            {
                OpponentPerk1Image.sprite = OpponentSelectedPerk1.DisplaySprite;
                OpponentPerk1Text.text = OpponentSelectedPerk1.DisplayName;
            }
            else
            {
                OpponentPerk1Image.sprite = NoPerkSprite;
                OpponentPerk1Text.text = "No Perk Selected";
            }

            if (OpponentSelectedPerk2 != null)
            {
                OpponentPerk2Image.sprite = OpponentSelectedPerk2.DisplaySprite;
                OpponentPerk2Text.text = OpponentSelectedPerk2.DisplayName;
            }
            else
            {
                OpponentPerk2Image.sprite = NoPerkSprite;
                OpponentPerk2Text.text = "No Perk Selected";
            }

            PlayerManager.Instance.SetMatchData(this);
        }

        public void ClearOpponentPerks()
        {
            OpponentSelectedPerk1 = null;
            OpponentSelectedPerk2 = null;

            OpponentPerk1Image.sprite = NoPerkSprite;
            OpponentPerk2Image.sprite = NoPerkSprite;

            OpponentPerk1Text.text = "No Perk Selected";
            OpponentPerk2Text.text = "No Perk Selected";
        }

        public void PlayButtonPressed()
        {
            if (!PlayerReady || !OpponentReady)
            {
                return;
            }

            PlayerButtonWasPressed = true;
            PlayerManager.Instance.SetMatchData(this);
            SceneLoadManager.Instance.LoadScene("Game", true);
        }

        public void ReadyButtonPressed()
        {
            if (PlayerSelectedRace == null || PlayerReady)
            {
                PlayerReadyObject.gameObject.SetActive(false);
                PlayerReady = false;
            }
            else
            {
                PlayerReadyObject.gameObject.SetActive(true);
                PlayerReady = true;
            }

            // RPC Call
            Debug.Log("Client Sent - Ready To Go - Custom Game");
            PlayerManager.Instance.NetworkHelper.SendReady(PlayerManager.Instance.LocalPlayer, PlayerReady);
        }


        #region Ready RPC

        #region Fishnet

        public void LocalPlayerReady(bool Ready)
        {
            PlayerReady = Ready;
            PlayerReadyObject.gameObject.SetActive(Ready);
        }

        public void OtherPlayerReady(bool Ready)
        {
            OpponentReady = Ready;
            OpponentReadyObject.gameObject.SetActive(Ready);
        }

        #endregion

        #endregion

        public void BackButtonPressed()
        {
            if (MultiplayerStatus == MultiplayStatus.Searching)
            {
                return;
            }

            if (MultiplayerStatus == MultiplayStatus.FoundMatch)
            {
                // Forfiet match?
                MultiplayerStatus = MultiplayStatus.NoSearching;
                if (PlayerManager.Instance.ClientState != LocalConnectionStates.Stopped)
                {
                    InstanceFinder.ClientManager.StopConnection();
                }
                if (PlayerManager.Instance.ServerState != LocalConnectionStates.Stopped)
                {
                    InstanceFinder.ServerManager.StopConnection(true);
                }
                gameObject.SetActive(false);
                PlayerManager.Instance.SteamLobbyManager.Leave();
                return;
            }

            MultiplayerStatus = MultiplayStatus.NoSearching;

            gameObject.SetActive(false);
        }

        public void InviteButtonPressed()
        {
            // Open Invite ui - Steam? Epic? Other?

        }

        public void SetDirectConnect(string Ip)
        {
            DirectConnectString = Ip;
        }

        public void DirectConnectButtonPressed()
        {
            if (DirectConnectString == "None")
            {
                return;
            }

            WaitingForDirectConnect();
        }

        public void HostButtonPressed()
        {
            CancelHostButton.gameObject.SetActive(true);
            HostButton.gameObject.SetActive(false);

            MultiplayerStatus = MultiplayStatus.Hosting;

            foreach (GameObject UiObject in SearchingUi)
            {
                UiObject.SetActive(true);
            }

            foreach (GameObject UiObject in NotSearchingUi)
            {
                UiObject.SetActive(false);
            }

            foreach (GameObject UiObject in OpponentUi)
            {
                UiObject.SetActive(false);
            }

            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
            PlayerManager.Instance.SpawnFishnetNetworkHelper();
            PlayerManager.Instance.SteamLobbyManager.Create();
            PlayerManager.Instance.RequestOpponentCustomGame(FoundCustomGameOpponent, CancelSearch);
        }

        public void OnRoomCreated(Lobby RoomCreated)
        {
            SteamLobbyRoomID.text = $"Room ID: {RoomCreated.id}";

            CurrentLobby = RoomCreated;
        }

        public void OnRoomNotCreated()
        {
            SteamLobbyRoomID.text = $"Room ID: No Room";
        }

        public void OnRoomJoined(Lobby RoomCreated)
        {
            Debug.Log($"Joined Room {RoomCreated.id}");
            SteamLobbyRoomID.text = $"Room ID: {RoomCreated.id}";
            CurrentLobby = RoomCreated;
        }

        public void CopyRoomIDToClipboard()
        {
            GUIUtility.systemCopyBuffer = $"{CurrentLobby.id}";
        }

        public void CancelButtonPressed()
        {
            CancelHostButton.gameObject.SetActive(false);
            HostButton.gameObject.SetActive(true);

            MultiplayerStatus = MultiplayStatus.NoSearching;

            foreach (GameObject UiObject in SearchingUi)
            {
                UiObject.SetActive(false);
            }

            foreach (GameObject UiObject in NotSearchingUi)
            {
                UiObject.SetActive(true);
            }

            foreach (GameObject UiObject in OpponentUi)
            {
                UiObject.SetActive(false);
            }


            if (PlayerManager.Instance.ClientState != LocalConnectionStates.Stopped)
            {
                InstanceFinder.ClientManager.StopConnection();
            }
            if (PlayerManager.Instance.ServerState != LocalConnectionStates.Stopped)
            {
                InstanceFinder.ServerManager.StopConnection(true);
            }
        }

        public void WaitingForInvitedPlayer()
        {
            PlayerManager.Instance.RequestOpponentCustomGame(FoundCustomGameOpponent, CancelSearch);
            MultiplayerStatus = MultiplayStatus.Searching;

            foreach (GameObject UiObject in SearchingUi)
            {
                UiObject.SetActive(true);
            }

            foreach (GameObject UiObject in NotSearchingUi)
            {
                UiObject.SetActive(false);
            }

            foreach (GameObject UiObject in OpponentUi)
            {
                UiObject.SetActive(false);
            }
        }

        public void WaitingForDirectConnect()
        {
            PlayerManager.Instance.RequestOpponentCustomGameDirectConnect(FoundCustomGameOpponent, CancelSearch, DirectConnectString);
            MultiplayerStatus = MultiplayStatus.Searching;

            foreach (GameObject UiObject in SearchingUi)
            {
                UiObject.SetActive(true);
            }

            foreach (GameObject UiObject in NotSearchingUi)
            {
                UiObject.SetActive(false);
            }

            foreach (GameObject UiObject in OpponentUi)
            {
                UiObject.SetActive(false);
            }

            PlayerManager.Instance.SpawnFishnetNetworkHelper();
        }

        public void FoundCustomGameOpponent(PlayerData Player)
        {
            OpponentPlayerData = Player;
            MultiplayerStatus = MultiplayStatus.FoundMatch;
            OpponentNameText.text = Player.UserName;
            LoadOpponentPlayer(null, null, null);

            foreach (GameObject UiObject in SearchingUi)
            {
                UiObject.SetActive(false);
            }

            foreach (GameObject UiObject in NotSearchingUi)
            {
                UiObject.SetActive(false);
            }

            foreach (GameObject UiObject in OpponentUi)
            {
                UiObject.SetActive(true);
            }

            SendGameData();
        }

        public void CancelSearch()
        {
            MultiplayerStatus = MultiplayStatus.NoSearching;

            foreach (GameObject UiObject in SearchingUi)
            {
                UiObject.SetActive(false);
            }

            foreach (GameObject UiObject in NotSearchingUi)
            {
                UiObject.SetActive(true);
            }

            foreach (GameObject UiObject in OpponentUi)
            {
                UiObject.SetActive(false);
            }
        }

        #region Game Data RPC
        public void SendGameData()
        {
            // RPC Call
            Debug.Log("Client Sent Race / Perk Data");

            int SelectedPerk1 = -1;
            int SelectedPerk2 = -1;
            if (PlayerSelectedRace != null)
            {
                SelectedPerk1 = PlayerSelectedRace.GetPerkIndex(PlayerSelectedPerk1);
                SelectedPerk2 = PlayerSelectedRace.GetPerkIndex(PlayerSelectedPerk2);
            }

            PreGameDataBroadcast MyData = new PreGameDataBroadcast()
            {
                SendFromPlayer = PlayerManager.Instance.LocalPlayer.PlayerID,
                SelectedRaceIndex = RaceSelector.Instance.GetRaceIndex(PlayerSelectedRace),
                SelectedPerk1Index = SelectedPerk1,
                SelectedPerk2Index = SelectedPerk2,
            };

            InstanceFinder.ClientManager.Broadcast(MyData);
        }

        #region Fishnet

        public void BroadCastPlayDataToServer(NetworkConnection conn, PreGameDataBroadcast Data)
        {
            NetworkObject nob = conn.FirstObject;
            if (nob == null)
            {
                return;
            }

            PreGameDataBroadcastResponse MyData = new PreGameDataBroadcastResponse()
            {
                SendFromPlayer = Data.SendFromPlayer,
                SelectedRaceIndex = Data.SelectedRaceIndex,
                SelectedPerk1Index = Data.SelectedPerk1Index,
                SelectedPerk2Index = Data.SelectedPerk2Index,
            };

            Debug.Log("Server Sent Race / Perk Data");
            InstanceFinder.ServerManager.Broadcast(nob, MyData);
        }

        public void BroadCastPlayDataToClients(PreGameDataBroadcastResponse Data)
        {
            //Debug.Log($"Recieved Race / Perk Data {Data.SendFromPlayer}");
            if (Data.SendFromPlayer != PlayerManager.Instance.LocalPlayer.PlayerID)
            {
                //Debug.Log($"Use Race / Perk Data");
                RaceDataScriptableObject SelectedRace;
                Perk SelectedPerk1;
                Perk SelectedPerk2;
                if (Data.SelectedRaceIndex == -1 || Data.SelectedRaceIndex >= RaceSelector.Instance.RaceDataList.Count)
                {
                    SelectedRace = null;
                }
                else
                {
                    SelectedRace = RaceSelector.Instance.RaceDataList[Data.SelectedRaceIndex];
                }

                if (SelectedRace == null)
                {
                    SelectedPerk1 = null;
                    SelectedPerk2 = null;
                }
                else
                {
                    if (Data.SelectedPerk1Index == -1 || Data.SelectedPerk1Index >= SelectedRace.PossiblePerks.Count)
                    {
                        SelectedPerk1 = null;
                    }
                    else
                    {
                        SelectedPerk1 = SelectedRace.PossiblePerks[Data.SelectedPerk1Index];
                    }

                    if (Data.SelectedPerk2Index == -1 || Data.SelectedPerk2Index >= SelectedRace.PossiblePerks.Count)
                    {
                        SelectedPerk2 = null;
                    }
                    else
                    {
                        SelectedPerk2 = SelectedRace.PossiblePerks[Data.SelectedPerk2Index];
                    }
                }

                Instance.LoadOpponentPlayer(SelectedRace, SelectedPerk1, SelectedPerk2);
            }
            //else
            //{
            //    Debug.Log($"Don't Use Race / Perk Data");
            //}
        }

        #endregion

        #region Darkrift

        public void RecievePlayerData(DarkRiftReader reader)
        {
            ushort PlayerID = reader.ReadUInt16();
            if (PlayerID == PlayerManager.Instance.LocalPlayer.PlayerID)
            {
                return;
            }

            int RaceIndex = reader.ReadInt32();
            int Perk1Index = reader.ReadInt32();
            int Perk2Index = reader.ReadInt32();

            RaceDataScriptableObject SelectedRace;
            Perk SelectedPerk1;
            Perk SelectedPerk2;
            if (RaceIndex == -1 || RaceIndex >= RaceSelector.Instance.RaceDataList.Count)
            {
                SelectedRace = null;
            }
            else
            {
                SelectedRace = RaceSelector.Instance.RaceDataList[RaceIndex];
            }

            if (SelectedRace == null)
            {
                SelectedPerk1 = null;
                SelectedPerk2 = null;
            }
            else
            {
                if (Perk1Index == -1 || Perk1Index >= SelectedRace.PossiblePerks.Count)
                {
                    SelectedPerk1 = null;
                }
                else
                {
                    SelectedPerk1 = SelectedRace.PossiblePerks[Perk1Index];
                }

                if (Perk2Index == -1 || Perk2Index >= SelectedRace.PossiblePerks.Count)
                {
                    SelectedPerk2 = null;
                }
                else
                {
                    SelectedPerk2 = SelectedRace.PossiblePerks[Perk2Index];
                }
            }

            Instance.LoadOpponentPlayer(SelectedRace, SelectedPerk1, SelectedPerk2);
        }

        #endregion

        #endregion
    }

    public struct PreGameDataBroadcast : IBroadcast
    {
        public ushort SendFromPlayer;
        public int SelectedRaceIndex;
        public int SelectedPerk1Index;
        public int SelectedPerk2Index;
    }

    public struct PreGameDataBroadcastResponse : IBroadcast
    {
        public ushort SendFromPlayer;
        public int SelectedRaceIndex;
        public int SelectedPerk1Index;
        public int SelectedPerk2Index;
    }

    public enum NetworkingTypes
    {
        Riptide,
        Fishynet,
        Darkrift2,
    }
}