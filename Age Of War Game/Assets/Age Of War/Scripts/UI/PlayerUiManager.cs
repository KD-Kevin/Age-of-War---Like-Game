using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;

public class PlayerUiManager : MonoBehaviour
{
    [Header("Buy Units")]
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

    [Header("Health and Info")]
    [SerializeField]
    private ProgressBar PlayerHealthBar;
    [SerializeField]
    private ProgressBar PlayerXpBar;
    [SerializeField]
    private ProgressBar PlayerArmorBar;
    [SerializeField]
    private ProgressBar EnemyHealthBar;
    [SerializeField]
    private ProgressBar EnemyArmorBar;
    [SerializeField]
    private TextMeshProUGUI CurrencyText;
    [SerializeField]
    private TextMeshProUGUI PopulationText;

    [Header("Level Up")]
    [SerializeField]
    private GameObject ConfirmLevelUpObject;

    [Header("Speed Control")]
    [SerializeField]
    private Sprite OneTimesSpeedSprite;
    [SerializeField]
    private Sprite TwoTimesSpeedSprite;
    [SerializeField]
    private GameObject OtherPlayerRequestedSpeedObject;
    [SerializeField]
    private Image OtherPlayerRequestedSpeedImage;
    [SerializeField]
    private Image YourSpeedImage;

    [Header("Store")]
    [SerializeField]
    private GameObject StorePageObject;

    [Header("Menu")]
    [SerializeField]
    private GameObject MenuPageObject;

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

    private void Start()
    {
        RefeshBaseUi();

        PlayerManager.Instance.SendReadyToStart();
    }

    public void ChangeBase(BaseBuildingData BuildingData, int Team)
    {
        BaseBuilding CurrentBuilding = BaseBuilding.TeamBuildings[Team];
        if (Team == 1)
        {
            PlayerHealthBar.maxValue = BuildingData.MaxHealth;
            PlayerHealthBar.ChangeValue(CurrentBuilding.BuildingData.Health);

            PlayerArmorBar.maxValue = BuildingData.MaxArmor;
            PlayerArmorBar.ChangeValue(CurrentBuilding.BuildingData.Armor);

            PlayerXpBar.maxValue = BuildingData.MaxExperience;
            PlayerXpBar.ChangeValue(CurrentBuilding.BuildingData.Experience);

            PlayerArmorBar.gameObject.SetActive(BuildingData.MaxArmor > 0);
        }
        else if (Team == 2)
        {
            EnemyHealthBar.maxValue = BuildingData.MaxHealth;
            EnemyHealthBar.ChangeValue(CurrentBuilding.BuildingData.Health);

            EnemyArmorBar.maxValue = BuildingData.MaxArmor;
            EnemyArmorBar.ChangeValue(CurrentBuilding.BuildingData.Armor);

            EnemyArmorBar.gameObject.SetActive(BuildingData.MaxArmor > 0);
        }

        BuildingData.Health = CurrentBuilding.BuildingData.Health;
        BuildingData.Armor = CurrentBuilding.BuildingData.Armor;
        BuildingData.Experience = CurrentBuilding.BuildingData.Experience;

        UpdatePopulationText();
    }

    public void RefeshBaseUi()
    {
        BaseBuilding PlayerBuilding = BaseBuilding.TeamBuildings[1];
        PlayerHealthBar.maxValue = PlayerBuilding.MaxHealth;
        PlayerHealthBar.ChangeValue(PlayerBuilding.BuildingData.Health);

        PlayerXpBar.maxValue = PlayerBuilding.MaxXp;
        PlayerXpBar.ChangeValue(PlayerBuilding.BuildingData.Experience);

        PlayerArmorBar.maxValue = PlayerBuilding.MaxArmor;
        PlayerArmorBar.ChangeValue(PlayerBuilding.BuildingData.Armor);

        PlayerArmorBar.gameObject.SetActive(PlayerBuilding.MaxArmor > 0);

        BaseBuilding EnemyBuilding = BaseBuilding.TeamBuildings[2];
        EnemyHealthBar.maxValue = EnemyBuilding.MaxHealth;
        EnemyHealthBar.ChangeValue(EnemyBuilding.BuildingData.Health);

        EnemyArmorBar.maxValue = EnemyBuilding.MaxArmor;
        EnemyArmorBar.ChangeValue(EnemyBuilding.BuildingData.Armor);

        EnemyArmorBar.gameObject.SetActive(EnemyBuilding.MaxArmor > 0);

        UpdatePopulationText();
        UpdateCurrencyText();
    }

    public void UpdatePopulationText()
    {
        BaseBuilding PlayerBuilding = BaseBuilding.TeamBuildings[1];
        PopulationText.text = $"{PlayerBuilding.GetPopulation()} / {PlayerBuilding.BuildingData.MaxPopulation}";
    }

    public void UpdateCurrencyText()
    {
        CurrencyText.text = $"${CurrentCurrencyAmount}";
    }

    public void BuyUnit(BaseUnitBehaviour unit)
    {
        BaseBuilding.TeamBuildings[1].BuyUnit(unit);
        UpdatePopulationText();
        UpdateCurrencyText();
    }

    public bool CanAffordUnit(BaseUnitBehaviour unit)
    {
        if (unit == null)
        {
            return false;
        }

        return CurrentCurrencyAmount >= unit.GetBuildCost();
    }

    public void GoToMainMenu()
    {
        NetworkManager.Instance.LeaveGame();
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }
}
