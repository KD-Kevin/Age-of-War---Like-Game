using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.ModernUIPack;
using AgeOfWar.Core;
using AgeOfWar.Data;

namespace AgeOfWar.UI
{
    public class PerkSelector : MonoBehaviour
    {
        [SerializeField]
        private Sprite NoPerkInSlotSprite;
        [SerializeField]
        private TextMeshProUGUI CurrentPerk1Text;
        [SerializeField]
        private Image CurrentPerk1Image;
        [SerializeField]
        private TextMeshProUGUI CurrentPerk2Text;
        [SerializeField]
        private Image CurrentPerk2Image;
        [SerializeField]
        private Image CurrentlyEdditingPerk1Image;
        [SerializeField]
        private Image CurrentlyEdditingPerk2Image;
        [SerializeField]
        private WindowManager CatagoryWindow;
        [SerializeField]
        private List<PerkSelectorCatagory> Catagories = new List<PerkSelectorCatagory>();

        public static PerkSelector Instance = null;
        public Perk SelectedPerk1 { get; set; }
        public Perk SelectedPerk2 { get; set; }
        public PerkSelectorCatagory CurrentCatagory { get; set; }
        public bool EdittingPerk1 { get; set; }
        public List<Perk> PossiblePerks { get; set; }

        private System.Action<Perk, Perk> ConfirmationCallback = null;
        private System.Action CancellationCallback = null;

        public void SetInstance()
        {
            Instance = this;
        }

        public void OpenPerkSelector(RaceDataScriptableObject OpenForRace, System.Action<Perk, Perk> ConfirmCallback, System.Action CancelCallback, Perk CurrentPerk1, Perk CurrentPerk2)
        {
            if (OpenForRace == null)
            {
                return;
            }

            PossiblePerks = OpenForRace.PossiblePerks;
            gameObject.SetActive(true);
            ConfirmationCallback = ConfirmCallback;
            CancellationCallback = CancelCallback;

            SelectedPerk1 = CurrentPerk1;
            if (SelectedPerk1 == null)
            {
                ClearPerk1();
                EditPerk1();
            }
            else
            {
                CurrentPerk1Image.sprite = SelectedPerk1.DisplaySprite;
                CurrentPerk1Text.text = SelectedPerk1.DisplayName;
                if (CurrentPerk2 == null)
                {
                    EdittingPerk1 = false;
                }
                else
                {
                    EdittingPerk1 = true;
                }
            }

            SelectedPerk2 = CurrentPerk2;
            if (SelectedPerk2 == null)
            {
                ClearPerk2();
                if (SelectedPerk1 != null)
                {
                    EditPerk2();
                }
            }
            else
            {
                CurrentPerk2Image.sprite = SelectedPerk2.DisplaySprite;
                CurrentPerk2Text.text = SelectedPerk2.DisplayName;
            }

            CatagoryWindow.OpenFirstTab();
            UpdateCatagory(Catagories[0]);

            ResetUi();
        }

        public void SelectPerk(PerkUiSelectionElement SelectUi)
        {
            if (SelectUi.CorrespondingPerk == SelectedPerk1 || SelectUi.CorrespondingPerk == SelectedPerk2)
            {
                return;
            }

            if (EdittingPerk1)
            {
                SelectedPerk1 = SelectUi.CorrespondingPerk;
                CurrentPerk1Image.sprite = SelectedPerk1.DisplaySprite;
                CurrentPerk1Text.text = SelectedPerk1.DisplayName;
                if (SelectedPerk2 == null)
                {
                    EdittingPerk1 = false;
                    CurrentlyEdditingPerk1Image.gameObject.SetActive(false);
                    CurrentlyEdditingPerk2Image.gameObject.SetActive(true);
                }
            }
            else
            {
                SelectedPerk2 = SelectUi.CorrespondingPerk;
                CurrentPerk2Image.sprite = SelectedPerk2.DisplaySprite;
                CurrentPerk2Text.text = SelectedPerk2.DisplayName;
                if (SelectedPerk1 == null)
                {
                    EdittingPerk1 = true;
                    CurrentlyEdditingPerk2Image.gameObject.SetActive(false);
                    CurrentlyEdditingPerk1Image.gameObject.SetActive(true);
                }
            }

            ResetUi();
        }

        public void ResetUi()
        {
            if (CurrentCatagory == null)
            {
                CurrentCatagory = Catagories[0];
            }
            CurrentCatagory.ResetUi(SelectedPerk1, SelectedPerk2);
        }

        public void UpdateCatagory(PerkSelectorCatagory Catagory)
        {
            if (!Catagories.Contains(Catagory))
            {
                return;
            }

            int IndexOfCatagory = Catagories.IndexOf(Catagory);
            CurrentCatagory = Catagory;

            CurrentCatagory.DisableCatagory();
            CurrentCatagory.EnableCatagory(PossiblePerks, (PerkTypes)IndexOfCatagory, SelectedPerk1, SelectedPerk2);
        }

        public void Confirm()
        {
            ConfirmationCallback(SelectedPerk1, SelectedPerk2);
            gameObject.SetActive(false);
        }

        public void CancelSelection()
        {
            CancellationCallback();
            gameObject.SetActive(false);
        }

        public void ClearPerk1()
        {
            SelectedPerk1 = null;
            CurrentPerk1Image.sprite = NoPerkInSlotSprite;
            CurrentPerk1Text.text = "No Perk Selected";
            ResetUi();
        }

        public void ClearPerk2()
        {
            SelectedPerk2 = null;
            CurrentPerk2Image.sprite = NoPerkInSlotSprite;
            CurrentPerk2Text.text = "No Perk Selected";
            ResetUi();
        }

        public void EditPerk1()
        {
            EdittingPerk1 = true;
            CurrentlyEdditingPerk1Image.gameObject.SetActive(true);
            CurrentlyEdditingPerk2Image.gameObject.SetActive(false);
        }

        public void EditPerk2()
        {
            EdittingPerk1 = false;
            CurrentlyEdditingPerk1Image.gameObject.SetActive(false);
            CurrentlyEdditingPerk2Image.gameObject.SetActive(true);
        }
    }
}
