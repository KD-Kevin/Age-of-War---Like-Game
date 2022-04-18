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
    [SerializeField]
    private TextMeshProUGUI UnitCost;

    public float BuildTimer { get; set; }
    public float BuildTime { get; set; }
    public float CooldownTimer { get; set; }
    public float CooldownTime { get; set; }
    public BaseUnitBehaviour BuyUnit { get; private set; }
    public int BuyUnitIndex { get; private set; }

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

            if (PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.CustomGame || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Quickplay || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Ranked)
            {
                // Send of Action to lockstep
                BuyUnitAction NewAction = new BuyUnitAction(BuyUnitIndex);
                LockstepManager.Instance.LocalPlayersCurrentTurn.AddAction(NewAction);
                return;
            }
            PlayerUiManager.Instance.CurrentCurrencyAmount -= BuyUnit.GetBuildCost();
            if (!BaseBuilding.TeamBuildings[1].HoldPopulationSpot.Contains(BuyUnitIndex))
            {
                BaseBuilding.TeamBuildings[1].HoldPopulationSpot.Add(BuyUnitIndex);
            }
            PlayerUiManager.Instance.UpdateCurrencyText();
            PlayerUiManager.Instance.UpdatePopulationText();
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
                    if (BaseBuilding.TeamBuildings[1].HoldPopulationSpot.Contains(BuyUnitIndex))
                    {
                        BaseBuilding.TeamBuildings[1].HoldPopulationSpot.Remove(BuyUnitIndex);
                    }
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

        if (BuyUnit != null)
        {
            UpdateText();
        }
    }

    public void SetUnit(BaseUnitBehaviour Unit, int UnitIndex)
    {
        if (Unit == null)
        {
            gameObject.SetActive(false);
            return;
        }
        BuyUnitIndex = UnitIndex;
        BuyUnit = Unit;
        UnitName.text = Unit.DisplayName;
        UnitSprite.sprite = Unit.DisplaySprite;
        UnitCost.text = $"${Unit.GetBuildCost()}";
    }

    public void UpdateText()
    {
        UnitCost.text = $"${BuyUnit.GetBuildCost()}";
    }
}

[System.Serializable]
public class BuyUnitAction : IAction
{
    public int ActionType { get; set; }
    public ushort OwningPlayer { get; set; }
    public int UnitBuyIndex { get; set; }

    public BuyUnitAction(int BuyIndex = -1)
    {
        ActionType = (int)ActionTypes.BuyUnit;
        OwningPlayer = PlayerManager.Instance.LocalPlayer.PlayerID;
        UnitBuyIndex = BuyIndex;
    }

    public void ProcessAction()
    {
        int CostToReduce;
        BaseUnitBehaviour UnitToBuy = PlayerUiManager.Instance.GetUnitByIndex(UnitBuyIndex);
        if (UnitToBuy != null)
        {
            CostToReduce = UnitToBuy.GetBuildCost();
        }
        else
        {
            return;
        }
        if (!BaseBuilding.TeamBuildings[OwningPlayer].HoldPopulationSpot.Contains(UnitBuyIndex))
        {
            BaseBuilding.TeamBuildings[OwningPlayer].HoldPopulationSpot.Add(UnitBuyIndex);
        }
        if (OwningPlayer != PlayerManager.Instance.LocalPlayer.PlayerID)
        {
            return;
        }
        PlayerUiManager.Instance.CurrentCurrencyAmount -= CostToReduce;
        PlayerUiManager.Instance.UpdateCurrencyText();
        PlayerUiManager.Instance.UpdatePopulationText();
        BuyUnitUI UI = PlayerUiManager.Instance.GetBuyUnitUiByIndex(UnitBuyIndex);

        if (UI == null)
        {
            return;
        }
        UI.CooldownTime = UnitToBuy.GetCooldown();
        UI.CooldownTimer = UI.CooldownTime;
        UI.BuildTimer = UnitToBuy.GetTimeToCreate();
        UI.BuildTime = UI.BuildTimer;
    }
}