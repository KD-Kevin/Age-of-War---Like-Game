using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using AgeOfWar.Core;
using AgeOfWar.Data;

namespace AgeOfWar.UI
{
    public class VersusAiPage : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI PlayerNameText;

        [SerializeField]
        private TextMeshProUGUI PlayerRaceText;
        [SerializeField]
        private Image PlayerRaceImage;
        [SerializeField]
        private TextMeshProUGUI AiRaceText;
        [SerializeField]
        private Image AiRaceImage;

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
        private TextMeshProUGUI AiPerk1Text;
        [SerializeField]
        private Image AiPerk1Image;
        [SerializeField]
        private Button AiPerk1Button;
        [SerializeField]
        private TextMeshProUGUI AiPerk2Text;
        [SerializeField]
        private Image AiPerk2Image;
        [SerializeField]
        private Button AiPerk2Button;

        [SerializeField]
        private Button PlayerButton;

        public RaceDataScriptableObject PlayerSelectedRace { get; set; }

        public Perk PlayerSelectedPerk1 { get; set; }
        public Perk PlayerSelectedPerk2 { get; set; }


        public RaceDataScriptableObject AiSelectedRace { get; set; }
        public Perk AiSelectedPerk1 { get; set; }
        public Perk AiSelectedPerk2 { get; set; }

        public CampaignDifficulty CurrentDifficulty { get; set; }

        private void Start()
        {
            CurrentDifficulty = CampaignDifficulty.Normal;
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
            PlayerNameText.text = PlayerManager.Instance.LocalPlayerData.Data.UserName;
            if (AiSelectedRace == null || PlayerSelectedRace == null)
            {
                PlayerButton.interactable = false;
            }
            else
            {
                PlayerButton.interactable = true;
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

            if (AiSelectedRace == null || PlayerSelectedRace == null)
            {
                PlayerButton.interactable = false;
            }
            else
            {
                PlayerButton.interactable = true;
            }
        }

        public void CancelPlayerRaceSelection()
        {

        }


        public void OpenRaceSelectorForAi()
        {
            RaceSelector.Instance.OpenRaceSelector(ConfirmAiRaceSelection, CancelAiRaceSelection);
        }

        public void ConfirmAiRaceSelection(RaceDataScriptableObject SelectedRace)
        {
            AiSelectedRace = SelectedRace;
            ClearAiPerks();
            AiPerk1Button.interactable = true;
            AiPerk2Button.interactable = true;

            AiRaceImage.sprite = SelectedRace.DisplaySprite;
            AiRaceText.text = SelectedRace.DisplayName;

            if (AiSelectedRace == null || PlayerSelectedRace == null)
            {
                PlayerButton.interactable = false;
            }
            else
            {
                PlayerButton.interactable = true;
            }
        }

        public void CancelAiRaceSelection()
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

        public void ClearAiPerks()
        {
            AiSelectedPerk1 = null;
            AiSelectedPerk2 = null;

            AiPerk1Image.sprite = NoPerkSprite;
            AiPerk2Image.sprite = NoPerkSprite;

            AiPerk1Text.text = "No Perk Selected";
            AiPerk2Text.text = "No Perk Selected";
        }


        public void PlayButtonPressed()
        {
            if (PlayerSelectedRace == null || AiSelectedRace == null)
            {
                return;
            }

            PlayerManager.Instance.LoadPageData(this);
            PlayerManager.Instance.StartMatch();
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

        public void OpenPerkSelectorForAi()
        {
            if (AiSelectedRace == null)
            {
                return;
            }
            PerkSelector.Instance.OpenPerkSelector(AiSelectedRace, ConfirmAiPerkSelection, CancelAiPerkSelection, AiSelectedPerk1, AiSelectedPerk2);
        }

        public void ConfirmAiPerkSelection(Perk SelectedPerk1, Perk SelectedPerk2)
        {
            AiSelectedPerk1 = SelectedPerk1;
            if (AiSelectedPerk1 != null)
            {
                AiPerk1Image.sprite = SelectedPerk1.DisplaySprite;
                AiPerk1Text.text = SelectedPerk1.DisplayName;
            }

            AiSelectedPerk2 = SelectedPerk2;
            if (AiSelectedPerk2 != null)
            {
                AiPerk2Image.sprite = SelectedPerk2.DisplaySprite;
                AiPerk2Text.text = SelectedPerk2.DisplayName;
            }
        }

        public void CancelAiPerkSelection()
        {

        }

        public void ChangeDifficulty(int NewDifficulty)
        {
            CurrentDifficulty = (CampaignDifficulty)NewDifficulty;
        }
    }
}
