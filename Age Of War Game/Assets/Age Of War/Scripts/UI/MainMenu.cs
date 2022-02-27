using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private RaceSelector RaceSelectionUI;
    [SerializeField]
    private PerkSelector PerkSelectionUI;

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

    }

    public void OpenRankedPage()
    {

    }

    public void OpenCustomGamePage()
    {

    }

    public void JoinQuickplayQueue()
    {

    }

    public void OpenCampaignPage()
    {

    }
}
