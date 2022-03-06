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
    private RaceSelector RaceSelectionUI;
    [SerializeField]
    private PerkSelector PerkSelectionUI;
    [SerializeField]
    private GameObject PlayerModeMenuObject;

    public static MainMenu Instance;

    private void Awake()
    {
        Instance = this;
        RaceSelectionUI.SetInstance();
        PerkSelectionUI.SetInstance();
    }

    public void CloseGame()
    {
        if (Application.isEditor)
        {
            EditorApplication.ExitPlaymode();
        }
        else
        {
            Application.Quit();
        }
    }

    public void OpenVsCpuPage()
    {
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
        CP.OpenPage();
    }

    public void FoundQuickplayOpponent(PlayerData Player)
    {

    }

    public void CancelQuickplaySearch()
    {

    }
}
