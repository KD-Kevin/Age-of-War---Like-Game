using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class RankedPage : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI PlayerNameText;
    [SerializeField]
    private TextMeshProUGUI PlayerRankText;

    [SerializeField]
    private TextMeshProUGUI PlayerRaceText;
    [SerializeField]
    private Image PlayerRaceImage;

    [SerializeField]
    private Sprite NoPerkSprite;
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
    private TextMeshProUGUI OpponentRankText;

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
    private Button BackButton;

    public RaceDataScriptableObject PlayerSelectedRace { get; set; }
    public Perk PlayerSelectedPerk1 { get; set; }
    public Perk PlayerSelectedPerk2 { get; set; }

    public RaceDataScriptableObject OpponentSelectedRace { get; set; }
    public Perk OpponentSelectedPerk1 { get; set; }
    public Perk OpponentSelectedPerk2 { get; set; }

    public MultiplayStatus MultiplayerStatus { get; set; }

    private void OnDisable()
    {
        if (MainMenu.Instance.MainMenuMode != MainMenuModes.FrontMenu)
        {
            MainMenu.Instance.MainMenuMode = MainMenuModes.FrontMenu;
        }
    }

    public void OpenPage()
    {
        gameObject.SetActive(true);
        PlayerNameText.text = PlayerManager.Instance.LocalPlayerData.Data.UserName;
        OpponentRaceButton.interactable = false;
        OpponentPerk1Button.interactable = false;
        OpponentPerk2Button.interactable = false;
        BackButton.interactable = true;

        if (MultiplayerStatus == MultiplayStatus.Searching)
        {
            foreach(GameObject UiObject in SearchingUi)
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

        OpponentRaceImage.sprite = OpponentSelectedRace.DisplaySprite;
        OpponentRaceText.text = OpponentSelectedRace.DisplayName;

        if (OpponentSelectedPerk1 != null)
        {
            OpponentPerk1Image.sprite = OpponentSelectedPerk1.DisplaySprite;
            OpponentPerk1Text.text = OpponentSelectedPerk1.DisplayName;
        }

        if (OpponentSelectedPerk2 != null)
        {
            OpponentPerk2Image.sprite = OpponentSelectedPerk2.DisplaySprite;
            OpponentPerk2Text.text = OpponentSelectedPerk2.DisplayName;
        }
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
        // Search For Opponent
        PlayerManager.Instance.RequestOpponentRanked(FoundRankedOpponent, CancelSearch);

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

    public void FoundRankedOpponent(PlayerData Player)
    {
        MainMenu.Instance.OpenRankedPage();
        BackButton.interactable = false;
        MultiplayerStatus = MultiplayStatus.FoundMatch;
        OpponentNameText.text = Player.UserName;

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

    public void CancelSearch()
    {
        BackButton.interactable = true;
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
}

public enum MultiplayStatus
{
    Offline,
    NoSearching,
    Searching,
    Hosting,
    FoundMatch,
    MatchStarted,
    MatchInProgress,
    MatchFinished,
}