using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyUnitUI : MonoBehaviour
{
    [SerializeField]
    private ProgressBar BuildProgressBar;
    [SerializeField]
    private Image CooldownImage;
    [SerializeField]
    private TextMeshProUGUI UnitName;
    [SerializeField]
    private Image UnitSprite;

    private float BuildTimer = 0;
    private float BuildTime = 0;
    private float CooldownTimer = 0;
    private float CooldownTime = 0;
    public BaseUnitBehaviour BuyUnit { get; private set; }

    public void OnClick()
    {
        if (PlayerUiManager.Instance.CanAffordUnit(BuyUnit))
        {
            if (CooldownTimer > 0 || BuildTimer > 0)
            {
                return;
            }

            BaseBuilding HomeBase = BaseBuilding.TeamBuildings[1];
            if (HomeBase.GetPopulation() >= HomeBase.BuildingData.MaxPopulation)
            {
                return;
            }

            CooldownTime = BuyUnit.GetCooldown();
            CooldownTimer = CooldownTime;
            BuildTimer = BuyUnit.GetTimeToCreate();
            BuildTime = BuildTimer;
        }
    }

    private void Update()
    {
        if (BuildTimer > 0)
        {
            if (!BuildProgressBar.gameObject.activeSelf)
            {
                BuildProgressBar.gameObject.SetActive(true);
            }

            BuildTimer -= Time.deltaTime;

            BuildProgressBar.ChangeValue(100 - BuildTimer * 100 / BuildTime);

            if (BuildTimer <= 0)
            {
                if (BuyUnit != null)
                {
                    PlayerUiManager.Instance.BuyUnit(BuyUnit);
                }
            }
        }
        else
        {
            if (BuildProgressBar.gameObject.activeSelf)
            {
                BuildProgressBar.gameObject.SetActive(false);
            }
        }

        if (CooldownTimer > 0)
        {
            if (!CooldownImage.gameObject.activeSelf)
            {
                CooldownImage.gameObject.SetActive(true);
            }

            CooldownTimer -= Time.deltaTime;

            CooldownImage.fillAmount = CooldownTimer / CooldownTime;
        }
        else
        {
            if (CooldownImage.gameObject.activeSelf)
            {
                CooldownImage.gameObject.SetActive(false);
            }
        }
    }

    public void SetUnit(BaseUnitBehaviour Unit)
    {
        BuyUnit = Unit;
        UnitName.text = Unit.DisplayName;
        UnitSprite.sprite = Unit.DisplaySprite;
    }
}
