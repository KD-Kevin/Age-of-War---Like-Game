using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.ModernUIPack;
using AgeOfWar.Data;
using AgeOfWar.Core.Units;

namespace AgeOfWar.UI
{
    public class RaceSelectionUiElement : MonoBehaviour
    {
        [SerializeField]
        private Color SelectionColor;
        [SerializeField]
        private Color IdleColor;
        [SerializeField]
        private Image RaceSpriteImage;
        [SerializeField]
        private TextMeshProUGUI RaceNameText;
        [SerializeField]
        private ButtonManagerBasic RaceNameButton;
        [SerializeField]
        private RaceSelectionUnitUiElement UnitUiPrefab;
        [SerializeField]
        private Transform PrefabSpawn;

        public RaceDataScriptableObject CorrespondingData { get; set; }

        private List<RaceSelectionUnitUiElement> UnitUiElements = new List<RaceSelectionUnitUiElement>();

        public void SetUi(RaceDataScriptableObject Data)
        {
            CorrespondingData = Data;
            RaceNameButton.buttonText = Data.DisplayName;
            RaceNameText.text = Data.DisplayName;
            RaceSpriteImage.sprite = Data.DisplaySprite;

            foreach (BaseUnitBehaviour RaceUnit in Data.StartingUnitsBlueprints)
            {
                RaceSelectionUnitUiElement NewElement = Instantiate(UnitUiPrefab, PrefabSpawn);
                NewElement.SetUi(RaceUnit);

                UnitUiElements.Add(NewElement);
            }
        }

        public void ResetUi()
        {
            RaceNameButton.buttonVar.targetGraphic.color = IdleColor;
        }

        public void OnSelect()
        {
            RaceNameButton.buttonVar.targetGraphic.color = SelectionColor;
            RaceSelector.Instance.SelectData(this);
        }
    }
}
