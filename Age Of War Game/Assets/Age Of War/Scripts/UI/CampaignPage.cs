using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using AgeOfWar.Data;
using AgeOfWar.Core;

namespace AgeOfWar.UI
{
    public class CampaignPage : MonoBehaviour
    {
        [SerializeField]
        private Sprite NoRaceChosenImage;
        [SerializeField]
        private Image RaceButtonImage;
        [SerializeField]
        private TextMeshProUGUI PlayersNameText;
        [SerializeField]
        private TextMeshProUGUI CurrentRaceText;
        [SerializeField]
        private Button PlayButton;
        [SerializeField]
        private Button StartNewSaveButton;
        [SerializeField]
        private Button SelectRaceButton;
        [SerializeField]
        private CampaignSaveDataUiElement CampaignSaveDataUiElementPrefab;
        [SerializeField]
        private Transform SaveEntryPrefabSpawn;
        [SerializeField]
        private TextMeshProUGUI YouWontDifficultyTexts;
        [SerializeField]
        private TextMeshProUGUI CurrentSaveTitleText;
        [SerializeField]
        private HorizontalSelector DifficultySelector;
        [SerializeField]
        private Button DifficultySelectorLeftButton;
        [SerializeField]
        private Button DifficultySelectorRightButton;
        [SerializeField]
        private TMP_InputField SaveTitleInputField;
        [SerializeField]
        private TMP_InputField SaveTitleInputFieldBottomLeft;
        [SerializeField]
        private Button StartNewSaveButtonBottomLeft;

        public CampaignDifficulty CurrentDifficulty { get; set; }
        public CampaignSaveData CurrentSaveData { get; private set; }
        public string CurrentSaveTitle { get; set; }
        private bool AlreadyOpenned = false;
        public static CampaignPage Instance;

        private void Start()
        {
            gameObject.SetActive(false);
            Instance = this;
        }

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
            CurrentSaveData = new CampaignSaveData();
            PlayButton.interactable = false;
            SelectRaceButton.interactable = false;
            StartNewSaveButton.interactable = true;
            //DifficultySelector.enabled = false;
            CurrentRaceText.text = "";
            CurrentSaveTitle = "";
            RaceButtonImage.sprite = NoRaceChosenImage;
            SaveTitleInputField.interactable = false;
            SaveTitleInputField.SetTextWithoutNotify("");
            SaveTitleInputFieldBottomLeft.SetTextWithoutNotify("");
            if (AlreadyOpenned)
            {
                while (DifficultySelector.index > DifficultySelector.defaultIndex)
                {
                    DifficultySelector.PreviousClick();
                }
                while (DifficultySelector.index < DifficultySelector.defaultIndex)
                {
                    DifficultySelector.ForwardClick();
                }
            }
            AlreadyOpenned = true;
            DifficultySelectorLeftButton.interactable = false;
            DifficultySelectorRightButton.interactable = false;
            CurrentDifficulty = CampaignDifficulty.Normal;
            SelectRaceButton.gameObject.SetActive(false);
            DifficultySelector.gameObject.SetActive(false);
            StartNewSaveButton.gameObject.SetActive(false);
            SaveTitleInputField.gameObject.SetActive(false);
            StartNewSaveButtonBottomLeft.gameObject.SetActive(true);
            SaveTitleInputFieldBottomLeft.gameObject.SetActive(true);
        }

        public void SelectNewSave()
        {
            SelectRaceButton.gameObject.SetActive(true);
            DifficultySelector.gameObject.SetActive(true);
            CurrentSaveData = new CampaignSaveData();
            CurrentSaveData.SaveDataTitle = CurrentSaveTitle;
            StartNewSaveButton.interactable = false;
            SelectRaceButton.interactable = true;
            //DifficultySelector.enabled = true;
            PlayButton.interactable = false;
            CurrentRaceText.text = "";
            RaceButtonImage.sprite = NoRaceChosenImage;
            SaveTitleInputField.interactable = true;
            SaveTitleInputField.SetTextWithoutNotify(CurrentSaveTitle);
            SaveTitleInputFieldBottomLeft.SetTextWithoutNotify(CurrentSaveTitle);
            DifficultySelectorLeftButton.interactable = true;
            DifficultySelectorRightButton.interactable = true;
            CurrentDifficulty = CampaignDifficulty.Normal;
            StartNewSaveButton.gameObject.SetActive(true);
            SaveTitleInputField.gameObject.SetActive(true);
            StartNewSaveButtonBottomLeft.gameObject.SetActive(false);
            SaveTitleInputFieldBottomLeft.gameObject.SetActive(false);
            Invoke(nameof(DelayedNewSave), 0.2f);
        }

        private void DelayedNewSave()
        {
            while (DifficultySelector.index > DifficultySelector.defaultIndex)
            {
                DifficultySelector.PreviousClick();
            }
            while (DifficultySelector.index < DifficultySelector.defaultIndex)
            {
                DifficultySelector.ForwardClick();
            }
        }

        public void LoadSaveData(CampaignSaveData SaveData)
        {
            CurrentSaveData = SaveData;
            SelectRaceButton.gameObject.SetActive(true);
            DifficultySelector.gameObject.SetActive(true);
            StartNewSaveButton.interactable = true;
            //DifficultySelector.enabled = false;
            SelectRaceButton.interactable = false;
            PlayButton.interactable = true;
            SaveTitleInputField.interactable = false;
            RaceButtonImage.sprite = SaveData.CampainRace.DisplaySprite;
            CurrentSaveTitleText.text = SaveData.SaveDataTitle;
            CurrentRaceText.text = SaveData.CampainRace.DisplayName;
            SaveTitleInputField.SetTextWithoutNotify(SaveData.SaveDataTitle);
            SaveTitleInputFieldBottomLeft.SetTextWithoutNotify(SaveData.SaveDataTitle);
            DifficultySelectorLeftButton.interactable = false;
            DifficultySelectorRightButton.interactable = false;
            CurrentSaveTitle = SaveData.SaveDataTitle;

            while (DifficultySelector.index > (int)SaveData.Difficulty)
            {
                DifficultySelector.PreviousClick();
            }
            while (DifficultySelector.index < (int)SaveData.Difficulty)
            {
                DifficultySelector.ForwardClick();
            }

            CurrentDifficulty = SaveData.Difficulty;
            StartNewSaveButton.gameObject.SetActive(true);
            SaveTitleInputField.gameObject.SetActive(true);
            StartNewSaveButtonBottomLeft.gameObject.SetActive(false);
            SaveTitleInputFieldBottomLeft.gameObject.SetActive(false);
        }

        public void DifficultyChange(int difficulty)
        {
            CurrentDifficulty = (CampaignDifficulty)difficulty;
            YouWontDifficultyTexts.gameObject.SetActive(CurrentDifficulty == CampaignDifficulty.YouWont);
            if (CurrentSaveData != null)
            {
                CurrentSaveData.Difficulty = CurrentDifficulty;
            }
        }

        public void SetSaveTitle(string Title)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                if (CurrentSaveData.CampainRace != null)
                {
                    PlayButton.interactable = true;
                }
            }
            else
            {
                PlayButton.interactable = false;
            }
            CurrentSaveTitle = Title;
            CurrentSaveData.SaveDataTitle = Title;
            SaveTitleInputField.SetTextWithoutNotify(Title);
            SaveTitleInputFieldBottomLeft.SetTextWithoutNotify(Title);
        }

        public void OpenRaceSelector()
        {
            RaceSelector.Instance.OpenRaceSelector(ConfirmRaceSelectionCallback, CancelRaceSelectionCallback);
        }

        public void ConfirmRaceSelectionCallback(RaceDataScriptableObject ConfirmedRace)
        {
            CurrentSaveData.CampainRace = ConfirmedRace;
            RaceButtonImage.sprite = ConfirmedRace.DisplaySprite;
            CurrentRaceText.text = ConfirmedRace.DisplayName;

            PlayButton.interactable = !string.IsNullOrEmpty(CurrentSaveData.SaveDataTitle);
        }

        public void CancelRaceSelectionCallback()
        {
            // Add things here later if we want cancelled race selection effects
        }

        public void PlayButtonPressed()
        {
            if (CurrentSaveData == null || CurrentSaveData.CampainRace == null || string.IsNullOrEmpty(CurrentSaveData.SaveDataTitle))
            {
                return;
            }

            PlayerManager.Instance.LoadPageData(this);
            PlayerManager.Instance.StartCampaign();
        }
    }

    [System.Serializable]
    public class CampaignSaveData
    {
        public string SaveDataTitle;
        public RaceDataScriptableObject CampainRace = null;
        public CampaignDifficulty Difficulty = CampaignDifficulty.Normal;

        // Add more stuff here when it becomes relevent
    }

    public enum CampaignDifficulty
    {
        VeryEasy,
        Easy,
        Normal,
        Hard,
        Insane,
        YouWont,
    }
}