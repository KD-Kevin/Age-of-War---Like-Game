using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    private void Awake()
    {
        Instance = this;
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
