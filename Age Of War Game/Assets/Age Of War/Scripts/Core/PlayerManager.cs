using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance = null;
    public PlayModes ActiveOnlineMode = PlayModes.None;

    // Use to store data for player and opponent
    public PlayerDataScriptableObject PlayerData;
    public PlayerDataScriptableObject CustomGameOpponentData;
    public PlayerDataScriptableObject QuickplayOpponentData;
    public PlayerDataScriptableObject RankedOpponentData;

    public MatchData CurrentMatchData = new MatchData();
    public List<(PlayerData, PlayModes)> PreviousPlayerData = new List<(PlayerData, PlayModes)>(); // Keeps track of your previous players you played against and in which modes
    public List<MatchData> PreviousMatchData = new List<MatchData>();

    // Campaign
    public CampaignSaveData CurrentCampaignSaveData { get; set; }

    // Online
    public NetworkPlayer LocalPlayer { get; set; }
    public Dictionary<ushort, NetworkPlayer> ConnectedPlayers = new Dictionary<ushort, NetworkPlayer>();

    // Custom Game
    private System.Action<PlayerData> ConfirmationCustomGameOpponentCallback = null;
    private System.Action CancellationCustomGameOpponentCallback = null;

    // Quickplay Game
    private System.Action<PlayerData> ConfirmationQuickplayOpponentCallback = null;
    private System.Action CancellationQuickplayOpponentCallback = null;

    // Ranked Game
    private System.Action<PlayerData> ConfirmationRankedOpponentCallback = null;
    private System.Action CancellationRankedCallback = null;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        // Load Player Data
    }

    // Online
    public void SendReadyToStart()
    {
        Debug.Log("Client Sent - Send Ready Confirmation");
        Message message = Message.Create(MessageSendMode.reliable, MessageId.SendPlayerReadyForSimulation);
        message.AddUShort(Instance.LocalPlayer.PlayerID);
        message.AddBool(true);
        NetworkManager.Instance.Client.Send(message);
    }

    [MessageHandler((ushort)MessageId.SendPlayerReadyForSimulation)]
    private static void SendReadyToStart(ushort fromClientId, Message message)
    {
        Debug.Log("Server Recieved - Send Ready Confirmation");
        ushort newPlayerId = message.GetUShort();
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.SendPlayerReadyForSimulation);
        bool Confirmed = message.GetBool();
        messageToSend.AddUShort(newPlayerId);
        messageToSend.AddBool(Confirmed);
        NetworkManager.Instance.Server.Send(messageToSend, newPlayerId);

        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            if (player.PlayerID == fromClientId)
            {
                player.ReadyToStart = Confirmed;
            }
        }
    }

    [MessageHandler((ushort)MessageId.SendPlayerReadyForSimulation)]
    private static void SendReadyToStart(Message message)
    {
        if (NetworkManager.Instance.IsHost)
        {
            return;
        }
        Debug.Log("Client Recieved - Send Ready Confirmation");
        ushort confirmedPlayer = message.GetUShort();
        bool Confirmed = message.GetBool();

        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            if (player.PlayerID == confirmedPlayer)
            {
                player.ReadyToStart = Confirmed;
            }
        }
    }

    public bool EveryoneIsReadyForStart()
    {
        if (ConnectedPlayers.Count <= 1)
        {
            return false;
        }

        foreach(NetworkPlayer player in ConnectedPlayers.Values)
        {
            if (!player.ReadyToStart)
            {
                return false;
            }
        }

        return true;
    }

    public void OtherPlayerJoined()
    {
        Debug.Log("Other Player Joined");
        if (ActiveOnlineMode == PlayModes.Ranked || ActiveOnlineMode == PlayModes.Quickplay || ActiveOnlineMode == PlayModes.CustomGame)
        {
            SendPlayerData();
        }
        else
        {
            Debug.LogWarning("Non online mode - should not be able to connect to other players");
            NetworkManager.Instance.LeaveGame();
        }
    }

    public void SendPlayerData()
    {
        Debug.Log("Client Sent - Send Player Data");
        Message message = Message.Create(MessageSendMode.reliable, MessageId.SendPlayerData);
        message.AddUShort(Instance.LocalPlayer.PlayerID);
        BinaryFormatter BF = new BinaryFormatter();
        byte[] PlayerDataByteArr;
        using (var ms = new MemoryStream())
        {
            BF.Serialize(ms, PlayerData.Data);
            PlayerDataByteArr = ms.ToArray();
        }
        message.AddBytes(PlayerDataByteArr, true, true);

        NetworkManager.Instance.Client.Send(message);
    }

    [MessageHandler((ushort)MessageId.SendPlayerData)]
    private static void SendPlayerData(ushort fromClientId, Message message)
    {
        Debug.Log("Server Recieved - Send Player Data");
        ushort newPlayerId = message.GetUShort();
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.SendPlayerData);
        byte[] PlayerDataArr = message.GetBytes();
        messageToSend.AddUShort(newPlayerId);
        messageToSend.AddBytes(PlayerDataArr, true ,true);
        NetworkManager.Instance.Server.Send(messageToSend, newPlayerId);

        PlayerData SentPlayersData;
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(PlayerDataArr, 0, PlayerDataArr.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            SentPlayersData = binForm.Deserialize(memStream) as PlayerData;
        }


    }

    [MessageHandler((ushort)MessageId.SendPlayerData)]
    private static void SendPlayerData(Message message)
    {
        if (NetworkManager.Instance.IsHost)
        {
            return;
        }
        Debug.Log("Client Recieved - Send Player Data");
        ushort newPlayerId = message.GetUShort();
        byte[] PlayerDataArr = message.GetBytes();

        PlayerData SentPlayersData;
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(PlayerDataArr, 0, PlayerDataArr.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            SentPlayersData = binForm.Deserialize(memStream) as PlayerData;
        }

        if (Instance.ActiveOnlineMode == PlayModes.Ranked)
        {
            Instance.ConfirmationRankedOpponentCallback(SentPlayersData);
        }
        else if (Instance.ActiveOnlineMode == PlayModes.Quickplay)
        {
            Instance.ConfirmationQuickplayOpponentCallback(SentPlayersData);
        }
        else if (Instance.ActiveOnlineMode == PlayModes.CustomGame)
        {
            Instance.ConfirmationCustomGameOpponentCallback(SentPlayersData);
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
        NetworkManager.Instance.JoinGame(directConnectIp);
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

    // Load Page Data
    public void LoadPageData(VersusAiPage page)
    {
        CurrentMatchData.PlayMode = PlayModes.VsComputer;
        CurrentMatchData.AiDifficulty = page.CurrentDifficulty;

        CurrentMatchData.PlayersData = PlayerData.Data;
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
        SceneManager.sceneLoaded += LoadGameScene;
    }

    public void StartCampaign()
    {
        if (CurrentCampaignSaveData == null || CurrentCampaignSaveData.CampainRace == null)
        {
            return;
        }

        SceneLoadManager.Instance.LoadScene(CurrentCampaignSaveData.CampainRace.CampainSceneName);
        SceneManager.sceneLoaded += LoadCampaignScene;
    }

    public void LoadGameScene(Scene loadedScene, LoadSceneMode Mode)
    {
        SceneManager.sceneLoaded -= LoadGameScene;
    }

    public void LoadCampaignScene(Scene loadedScene, LoadSceneMode Mode)
    {
        SceneManager.sceneLoaded -= LoadCampaignScene;
    }
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
