using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private CampaignPage CP;
    [SerializeField]
    private VersusAiPage AiP;
    [SerializeField]
    private RankedPage RP;
    [SerializeField]
    private CustomGamePage CGP;
    [SerializeField]
    private EnterUsernamePage EUP;
    [SerializeField]
    private RaceSelector RaceSelectionUI;
    [SerializeField]
    private PerkSelector PerkSelectionUI;
    [SerializeField]
    private GameObject PlayerModeMenuObject;

    public static MainMenu Instance = null;
    public MainMenuModes MainMenuMode = MainMenuModes.FrontMenu;

    public CampaignPage CampaignUi { get { return CP; } }
    public VersusAiPage VersusAiUi { get { return AiP; } }
    public RankedPage RankedUi { get { return RP; } }
    public CustomGamePage CustomGameUi { get { return CGP; } }
    public EnterUsernamePage EnterUsernameUi { get { return EUP; } }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Awake()
    {
        Instance = this;
        RaceSelectionUI.SetInstance();
        PerkSelectionUI.SetInstance();
    }

    private void Start()
    {
        if (PlayerManager.Instance.LocalPlayerData.Data.UserName == "No Name" || string.IsNullOrEmpty(PlayerManager.Instance.LocalPlayerData.Data.UserName))
        {
            EnterUsernameUi.OpenPage();
        }
    }

    public void CloseGame()
    {
        NetworkManager.Instance.LeaveGame();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif

        Application.Quit();

    }

    public void OpenVsCpuPage()
    {
        MainMenuMode = MainMenuModes.VersusAi;
        RP.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        CGP.gameObject.SetActive(false);
        CP.gameObject.SetActive(false);
        PlayerModeMenuObject.SetActive(true);
        AiP.OpenPage();
    }

    public void OpenRankedPage()
    {
        MainMenuMode = MainMenuModes.Ranked;
        AiP.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        CGP.gameObject.SetActive(false);
        CP.gameObject.SetActive(false);
        PlayerModeMenuObject.SetActive(true);
        RP.OpenPage();
    }

    public void OpenCustomGamePage()
    {
        MainMenuMode = MainMenuModes.Custom;
        AiP.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        RP.gameObject.SetActive(false);
        CP.gameObject.SetActive(false);
        PlayerModeMenuObject.SetActive(true);
        CGP.OpenPage();
    }

    public void JoinQuickplayQueue()
    {
        MainMenuMode = MainMenuModes.Quickplay;
        PlayerManager.Instance.RequestOpponentQuickplay(FoundQuickplayOpponent, CancelQuickplaySearch);
    }

    public void OpenCampaignPage()
    {
        AiP.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        RP.gameObject.SetActive(false);
        CGP.gameObject.SetActive(false);
        PlayerModeMenuObject.SetActive(false);
        MainMenuMode = MainMenuModes.Campaign;
        CP.OpenPage();
    }

    public void FoundQuickplayOpponent(PlayerData Player)
    {
        AiP.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        PerkSelectionUI.gameObject.SetActive(false);
        RP.gameObject.SetActive(false);
        CGP.gameObject.SetActive(false);
        PlayerModeMenuObject.SetActive(false);

        // Need to open page for the quickplay class selection and perk selections
    }

    public void CancelQuickplaySearch()
    {
        if (CP.gameObject.activeSelf)
        {
            MainMenuMode = MainMenuModes.Campaign;
        }
        else if (AiP.gameObject.activeSelf)
        {
            MainMenuMode = MainMenuModes.VersusAi;
        }
        else if (RP.gameObject.activeSelf)
        {
            MainMenuMode = MainMenuModes.Ranked;
        }
        else if (CGP.gameObject.activeSelf)
        {
            MainMenuMode = MainMenuModes.Custom;
        }
        else
        {
            MainMenuMode = MainMenuModes.FrontMenu;
        }
    }
}

public enum MainMenuModes
{
    FrontMenu,
    Quickplay,
    Ranked,
    Custom,
    VersusAi,
    Campaign,
}
