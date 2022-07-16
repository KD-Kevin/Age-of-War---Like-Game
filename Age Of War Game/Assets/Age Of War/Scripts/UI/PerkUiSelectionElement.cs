using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgeOfWar.Core;

namespace AgeOfWar.UI
{
    public class PerkUiSelectionElement : MonoBehaviour
    {
        [SerializeField]
        private Color SelectedColor;
        [SerializeField]
        private Color IdleColor;
        [SerializeField]
        private Image PerkSpriteImage;
        [SerializeField]
        private TextMeshProUGUI PerkNameText;

        public Perk CorrespondingPerk { get; set; }
        public PerkSelectorCatagory CorrespondingCatagory { get; set; }

        public void SetUi(Perk PerkData)
        {
            CorrespondingPerk = PerkData;
            PerkNameText.text = PerkData.DisplayName;
            PerkSpriteImage.sprite = PerkData.DisplaySprite;
        }

        public void OnSelect()
        {
            PerkSpriteImage.color = SelectedColor;
            CorrespondingCatagory.OnSelect(this);
        }

        public void ResetUi()
        {
            PerkSpriteImage.color = IdleColor;
        }

        public void SetSelectionColor()
        {
            PerkSpriteImage.color = SelectedColor;
        }
    }
}
