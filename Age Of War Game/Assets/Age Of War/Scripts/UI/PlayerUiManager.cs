using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUiManager : MonoBehaviour
{
    [SerializeField]
    private BaseUnitBehaviour TestUnit;
    [SerializeField]
    private BuyUnitUI BuyBaseUnitUI;
    [SerializeField]
    private BuyUnitUI BuyRangedUnitUI;
    [SerializeField]
    private BuyUnitUI BuySpeedUnitUI;
    [SerializeField]
    private BuyUnitUI BuyTankUnitUI;
    [SerializeField]
    private BuyUnitUI BuyExtraUnitUI;

    public static PlayerUiManager Instance;

    public int CurrentCurrencyAmount { get; set; }

    private void Awake()
    {
        Instance = this;
        CurrentCurrencyAmount = 1000;

        BuyBaseUnitUI.SetUnit(TestUnit);
        BuyRangedUnitUI.SetUnit(TestUnit);
        BuySpeedUnitUI.SetUnit(TestUnit);
        BuyTankUnitUI.SetUnit(TestUnit);
        BuyExtraUnitUI.SetUnit(TestUnit);
    }

    public void BuyUnit(BaseUnitBehaviour unit)
    {
        BaseBuilding.TeamBuildings[1].BuyUnit(unit);
    }

    public bool CanAffordUnit(BaseUnitBehaviour unit)
    {
        if (unit == null)
        {
            return false;
        }

        return CurrentCurrencyAmount >= unit.GetBuildCost();
    }
}
