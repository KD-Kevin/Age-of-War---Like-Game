using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgeOfWar.Core.Units;

namespace AgeOfWar.UI
{
    public class RaceSelectionUnitUiElement : MonoBehaviour
    {
        [SerializeField]
        private Image UnitSpriteImage;
        [SerializeField]
        private TextMeshProUGUI UnitNameText;

        public void SetUi(BaseUnitBehaviour PrefabData)
        {
            UnitNameText.text = PrefabData.DisplayName;
            UnitSpriteImage.sprite = PrefabData.DisplaySprite;
        }
    }
}
