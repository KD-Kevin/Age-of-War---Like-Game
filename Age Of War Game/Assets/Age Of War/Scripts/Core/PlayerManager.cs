using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance = null;

    // Use to store data for player and opponent
    public PlayerDataScriptableObject PlayerData;
    public PlayerDataScriptableObject CustomGameOpponentData;
    public PlayerDataScriptableObject QuickplayOpponentData;
    public PlayerDataScriptableObject RankedOpponentData;

    public MatchData CurrentMatchData = new MatchData();
    public List<(PlayerData, PlayModes)> PreviousPlayerData = new List<(PlayerData, PlayModes)>(); // Keeps track of your previous players you played against and in which modes
    public List<MatchData> PreviousMatchData = new List<MatchData>();

    // Custom Game
    private System.Action<PlayerData> ConfirmationCustomGameOpponentCallback = null;
    private System.Action CancellationCustomGameOpponentCallback = null;

    // Quickplay Game
    private System.Action<PlayerData> ConfirmationQuickplayOpponentCallback = null;
    private System.Action CancellationQuickplayOpponentCallback = null;

    // Ranked Game
    private System.Action<PlayerData> ConfirmationRankedOpponentCallback = null;
    private System.Action CancellationRankedCallback = null;

    // Campaign
    public CampaignSaveData CurrentCampaignSaveData { get; set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Load Player Data
    }

    // Quickplay
    public void RequestOpponentQuickplay(System.Action<PlayerData> FindPlayer, System.Action CannotFindPlayer)
    {
        ConfirmationCustomGameOpponentCallback = FindPlayer;
        CancellationCustomGameOpponentCallback = CannotFindPlayer;
    }

    public void FoundQuickplayOpponent(PlayerData Player)
    {
        QuickplayOpponentData.Data = Player;
        ConfirmationCustomGameOpponentCallback.Invoke(Player);
    }

    public void CancelFindQuickplayOpponent()
    {
        CancellationCustomGameOpponentCallback.Invoke();
    }


    // Custom Game
    public void RequestOpponentCustomGame(System.Action<PlayerData> FindPlayer, System.Action CannotFindPlayer)
    {
        ConfirmationQuickplayOpponentCallback = FindPlayer;
        CancellationQuickplayOpponentCallback = CannotFindPlayer;
    }

    public void FoundCustomGameOpponent(PlayerData Player)
    {
        CustomGameOpponentData.Data = Player;
        ConfirmationQuickplayOpponentCallback.Invoke(Player);
    }

    public void CancelFindCustomGameOpponent()
    {
        CancellationQuickplayOpponentCallback.Invoke();
    }

    // Ranked Game
    public void RequestOpponentRanked(System.Action<PlayerData> FindPlayer, System.Action CannotFindPlayer)
    {
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
