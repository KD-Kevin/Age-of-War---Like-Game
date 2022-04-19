using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using RiptideNetworking;

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

    private void Awake()
    {
        PlayerButtonWasPressed = false;
        Instance = this;
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
        PlayButton.interactable = NetworkManager.Instance.IsHost && !PlayerButtonWasPressed;
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
        Message message = Message.Create(MessageSendMode.reliable, MessageId.ReadyButtonPressed);
        message.AddUShort(PlayerManager.Instance.LocalPlayer.PlayerID);
        message.AddBool(PlayerReady);
        NetworkManager.Instance.Client.Send(message);
    }

    [MessageHandler((ushort)MessageId.ReadyButtonPressed)]
    private static void ReadyButtonPressed(ushort fromClientId, Message message)
    {
        Debug.Log("Server Recieved - Ready To Go - Custom Game");
        ushort newPlayerId = message.GetUShort();
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.ReadyButtonPressed);
        bool Ready = message.GetBool();
        messageToSend.AddUShort(newPlayerId);
        messageToSend.AddBool(Ready);
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            if (player == PlayerManager.Instance.LocalPlayer)
            {
                continue;
            }
            NetworkManager.Instance.Server.Send(messageToSend, player.PlayerID);
        }

        if (newPlayerId == PlayerManager.Instance.LocalPlayer.PlayerID)
        {
            Instance.PlayerReady = Ready;
            Instance.PlayerReadyObject.gameObject.SetActive(Ready);
        }
        else
        {
            Instance.OpponentReady = Ready;
            Instance.OpponentReadyObject.gameObject.SetActive(Ready);
        }
    }

    [MessageHandler((ushort)MessageId.ReadyButtonPressed)]
    private static void SendConfirmation(Message message)
    {
        if (NetworkManager.Instance.IsHost)
        {
            return;
        }
        ushort confirmedPlayer = message.GetUShort();
        
        bool Ready = message.GetBool();
        Debug.Log("Client Recieved - Ready To Go - Custom Game");

        if (confirmedPlayer == PlayerManager.Instance.LocalPlayer.PlayerID)
        {
            Instance.PlayerReady = Ready;
            Instance.PlayerReadyObject.gameObject.SetActive(Ready);
        }
        else
        {
            Instance.OpponentReady = Ready;
            Instance.OpponentReadyObject.gameObject.SetActive(Ready);
        }
    }

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
            NetworkManager.Instance.LeaveGame();
            gameObject.SetActive(false);
            return;
        }

        MultiplayerStatus = MultiplayStatus.NoSearching;
        if (NetworkManager.Instance.Server.IsRunning)
        {
            NetworkManager.Instance.LeaveGame();
        }

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

        NetworkManager.Instance.StartHost();
        PlayerManager.Instance.RequestOpponentCustomGame(FoundCustomGameOpponent, CancelSearch);
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

        NetworkManager.Instance.LeaveGame();
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

    public void SendGameData()
    {
        // RPC Call
        Debug.Log("Client Sent - Send Game Data - Custom Game");
        Message message = Message.Create(MessageSendMode.reliable, MessageId.SendCustomGameRacePerkData);
        message.AddUShort(PlayerManager.Instance.LocalPlayer.PlayerID);
        message.AddInt(RaceSelector.Instance.GetRaceIndex(PlayerSelectedRace));
        if (PlayerSelectedRace != null)
        {
            message.AddInt(PlayerSelectedRace.GetPerkIndex(PlayerSelectedPerk1));
            message.AddInt(PlayerSelectedRace.GetPerkIndex(PlayerSelectedPerk2));
        }
        else
        {
            message.AddInt(-1);
            message.AddInt(-1);
        }

        NetworkManager.Instance.Client.Send(message);
    }

    [MessageHandler((ushort)MessageId.SendCustomGameRacePerkData)]
    private static void SendGameData(ushort fromClientId, Message message)
    {
        Debug.Log("Server Recieved - Send Game Data - Custom Game");
        ushort newPlayerId = message.GetUShort();
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.SendCustomGameRacePerkData);
        int RaceIndex = message.GetInt();
        int Perk1Index = message.GetInt();
        int Perk2Index = message.GetInt();
        messageToSend.AddUShort(newPlayerId);
        messageToSend.AddInt(RaceIndex);
        messageToSend.AddInt(Perk1Index);
        messageToSend.AddInt(Perk2Index);
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            if (player.PlayerID == fromClientId)
            {
                continue;
            }
            NetworkManager.Instance.Server.Send(messageToSend, player.PlayerID);
        }


        if (fromClientId == PlayerManager.Instance.LocalPlayer.PlayerID)
        {
            return;
        }

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

    [MessageHandler((ushort)MessageId.SendCustomGameRacePerkData)]
    private static void SendGameData(Message message)
    {
        Debug.Log("Client Recieved - Send Game Data - Custom Game");
        if (NetworkManager.Instance.IsHost)
        {
            return;
        }
        ushort newPlayerId = message.GetUShort();
        int RaceIndex = message.GetInt();
        int Perk1Index = message.GetInt();
        int Perk2Index = message.GetInt();

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
}
