using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
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

    public BaseUnitBehaviour Unit { get; private set; }

    public void SetUnit(BaseUnitBehaviour newUnit)
    {
        Unit = newUnit;

        if (Unit.MaxMagicArmor > 0)
        {
            MagicArmorUiObject.SetActive(true);
        }

        if (Unit.MaxArmor > 0)
        {
            ArmorUiObject.SetActive(true);
        }

        UpdateUi();
    }

    public void UpdateUi()
    {
        // Set Position
        transform.position = Unit.HealthBarTransform.position;

        // Set Values
        HealthFillImage.fillAmount = (float)Unit.CurrentHealth / Unit.MaxHealth;

        if (Unit.MaxArmor > 0)
        {
            ArmorFillImage.fillAmount = (float)Unit.CurrentArmor / Unit.MaxArmor;
        }
        else if (ArmorUiObject.activeInHierarchy)
        {
            ArmorUiObject.SetActive(false);
        }

        if (Unit.MaxMagicArmor > 0)
        {
            MagicalArmorFillImage.fillAmount = (float)Unit.CurrentMagicArmor / Unit.MaxMagicArmor;
        }
        else if (MagicArmorUiObject.activeInHierarchy)
        {
            MagicArmorUiObject.SetActive(false);
        }

        if (HealthFillImage.fillAmount <= 0)
        {
            UnitHealthBarManager.Instance.ReturnUi(this);
        }
    }


}
