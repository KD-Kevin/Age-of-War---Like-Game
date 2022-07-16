using AgeOfWar.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AgeOfWar.UI
{
    public class UnitHealthBar : UnitHealBarBase
    {
        [SerializeField]
        private Image HealthFillImage;
        [SerializeField]
        private GameObject ArmorUiObject;
        [SerializeField]
        private Image ArmorFillImage;
        [SerializeField]
        private GameObject MagicArmorUiObject;
        [SerializeField]
        private Image MagicalArmorFillImage;

        public override void SetUnit(IHealth newUnit)
        {
            base.SetUnit(newUnit);

            if (HealthComponent.MaxMagicArmor > 0)
            {
                MagicArmorUiObject.SetActive(true);
            }

            if (HealthComponent.MaxArmor > 0)
            {
                ArmorUiObject.SetActive(true);
            }

            UpdateUi();
        }

        public override void UpdateUi()
        {
            // Set Position
            base.UpdateUi();

            // Set Values
            HealthFillImage.fillAmount = (float)HealthComponent.CurrentHealth / HealthComponent.MaxHealth;

            if (HealthComponent.MaxArmor > 0)
            {
                ArmorFillImage.fillAmount = (float)HealthComponent.CurrentArmor / HealthComponent.MaxArmor;
            }
            else if (ArmorUiObject.activeInHierarchy)
            {
                ArmorUiObject.SetActive(false);
            }

            if (HealthComponent.MaxMagicArmor > 0)
            {
                MagicalArmorFillImage.fillAmount = (float)HealthComponent.CurrentMagicArmor / HealthComponent.MaxMagicArmor;
            }
            else if (MagicArmorUiObject.activeInHierarchy)
            {
                MagicArmorUiObject.SetActive(false);
            }

            if (HealthFillImage.fillAmount <= 0)
            {
                UnitHealthBarManager.Instance?.ReturnUi(this);
            }
        }


    }
}
