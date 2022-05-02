using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet.Broadcast;

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
    [SerializeField]
    private Button UI_Button;

    public float BuildTimer { get; set; }
    public float BuildTime { get; set; }
    public float CooldownTimer { get; set; }
    public float CooldownTime { get; set; }
    public BaseUnitBehaviour BuyUnit { get; private set; }
    public int BuyUnitIndex { get; private set; }
    public float ClickCooldownTimer { get; set; }

    public static List<BuyUnitUI> AllUnitBuyUi = new List<BuyUnitUI>();

    private void OnDestroy()
    {
        if (AllUnitBuyUi.Contains(this))
        {
            AllUnitBuyUi.Remove(this);
        }
    }

    private void OnDisable()
    {
        if (AllUnitBuyUi.Contains(this))
        {
            AllUnitBuyUi.Remove(this);
        }
    }

    private void OnEnable()
    {
        if (!AllUnitBuyUi.Contains(this))
        {
            AllUnitBuyUi.Add(this);
        }
    }

    private void Awake()
    {
        if (!AllUnitBuyUi.Contains(this))
        {
            AllUnitBuyUi.Add(this);
        }
    }

    public void OnClick()
    {
        if (ClickCooldownTimer > 0)
        {
            return;
        }

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

            ClickCooldownTimer = 1;
            UI_Button.interactable = false;
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

    public static void UpdateAllBuyUi()
    {
        foreach(BuyUnitUI ui in AllUnitBuyUi)
        {
            ui.UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (BuildTimer > 0)
        {
            if (!BuildProgressBar.gameObject.activeSelf)
            {
                BuildProgressBar.gameObject.SetActive(true);
            }

            BuildTimer -= LockstepManager.Instance.StepTime;

            BuildProgressBar.ChangeValue(100 - BuildTimer * 100 / BuildTime);

            if (BuildTimer <= 0)
            {
                if (BuyUnit != null)
                {
                    if (PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.CustomGame || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Quickplay || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Ranked)
                    {
                        if (BaseBuilding.TeamBuildings[PlayerManager.Instance.LocalPlayer.PlayerID].HoldPopulationSpot.Contains(BuyUnitIndex))
                        {
                            BaseBuilding.TeamBuildings[PlayerManager.Instance.LocalPlayer.PlayerID].HoldPopulationSpot.Remove(BuyUnitIndex);
                        }
                    }
                    else
                    {
                        if (BaseBuilding.TeamBuildings[1].HoldPopulationSpot.Contains(BuyUnitIndex))
                        {
                            BaseBuilding.TeamBuildings[1].HoldPopulationSpot.Remove(BuyUnitIndex);
                        }
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

            CooldownTimer -= LockstepManager.Instance.StepTime;

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

        if (ClickCooldownTimer > 0)
        {
            ClickCooldownTimer -= LockstepManager.Instance.StepTime;
            if (ClickCooldownTimer <= 0)
            {
                UI_Button.interactable = true;
            }
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
public class BuildOrder
{
    public float BuildTime;
    public BaseUnitBehaviour Order;
    public int OwnerID;

    public BuildOrder(BaseUnitBehaviour BuildOrder = null, float TimeToBuild = 0, int Owner = 0)
    {
        Order = BuildOrder;
        BuildTime = TimeToBuild;
        OwnerID = Owner;
        BaseBuilding.AddBuildOrder(this);
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
        if (OwningPlayer == PlayerManager.Instance.LocalPlayer.PlayerID)
        {
            BaseUnitBehaviour UnitToBuy = PlayerUiManager.Instance.GetUnitByIndex(UnitBuyIndex);
            int CostToReduce;
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
        else
        {
            BaseUnitBehaviour UnitToSpawn = PlayerManager.Instance.GetUnitByIndex(UnitBuyIndex, OwningPlayer);
            if (UnitToSpawn != null)
            {
                float TimeToBuild = UnitToSpawn.GetTimeToCreate();
                BuildOrder NewBuildOrder = new BuildOrder(UnitToSpawn, TimeToBuild, OwningPlayer);
            }
        }
    }
}

public struct BuyUnitActionBroadcast : IBroadcast
{
    public int TurnNumber;
    public int NumberOfActions;
    public ushort SentByPlayer;
    public int BuyIndex;
}