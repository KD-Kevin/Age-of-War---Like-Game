using Michsky.UI.ModernUIPack;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using AgeOfWar.Core.Units;
using AgeOfWar.Data;
using AgeOfWar.Core;

namespace AgeOfWar.UI
{
    public class PlayerUiManager : MonoBehaviour
    {
        [Header("Buy Units")]
        [SerializeField]
        private BaseUnitBehaviour TestUnit;
        [SerializeField]
        private BuyUnitUI BuyBaseUnitUI; // Index 0
        [SerializeField]
        private BuyUnitUI BuyRangedUnitUI; // Index 1
        [SerializeField]
        private BuyUnitUI BuySpeedUnitUI; // Index 2
        [SerializeField]
        private BuyUnitUI BuyTankUnitUI; // Index 3
        [SerializeField]
        private BuyUnitUI BuyExtraUnitUI; // Index 4

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
        public RaceDataScriptableObject SetRace { get; set; }

        private void Awake()
        {
            Instance = this;
            CurrentCurrencyAmount = 1000;

            SetRace = PlayerManager.Instance.CurrentMatchData.PlayerSelectedRace;
            if (SetRace != null)
            {
                if (SetRace.StartingUnitsBlueprints.Count > 0)
                {
                    BuyBaseUnitUI.SetUnit(SetRace.StartingUnitsBlueprints[0], 0);
                }
                else
                {
                    BuyBaseUnitUI.SetUnit(null, 0);
                }
                if (SetRace.StartingUnitsBlueprints.Count > 1)
                {
                    BuyRangedUnitUI.SetUnit(SetRace.StartingUnitsBlueprints[1], 1);
                }
                else
                {
                    BuyRangedUnitUI.SetUnit(null, 1);
                }
                if (SetRace.StartingUnitsBlueprints.Count > 2)
                {
                    BuySpeedUnitUI.SetUnit(SetRace.StartingUnitsBlueprints[2], 2);
                }
                else
                {
                    BuySpeedUnitUI.SetUnit(null, 2);
                }
                if (SetRace.StartingUnitsBlueprints.Count > 3)
                {
                    BuyTankUnitUI.SetUnit(SetRace.StartingUnitsBlueprints[3], 3);
                }
                else
                {
                    BuyTankUnitUI.SetUnit(null, 3);
                }
                if (SetRace.StartingUnitsBlueprints.Count > 4)
                {
                    BuyExtraUnitUI.SetUnit(SetRace.StartingUnitsBlueprints[4], 4);
                }
                else
                {
                    BuyExtraUnitUI.SetUnit(null, 4);
                }
            }
            else
            {
                BuyBaseUnitUI.SetUnit(TestUnit, 0);
                BuyRangedUnitUI.SetUnit(TestUnit, 1);
                BuySpeedUnitUI.SetUnit(TestUnit, 2);
                BuyTankUnitUI.SetUnit(TestUnit, 3);
                BuyExtraUnitUI.SetUnit(TestUnit, 4);
            }

            if (PlayerManager.Instance.ActiveOnlineMode == PlayModes.None || PlayerManager.Instance.ActiveOnlineMode == PlayModes.VsComputer)
            {
                PlayerManager.Instance.AddAi(2);
            }
        }

        public BaseUnitBehaviour GetUnitByIndex(int Index)
        {
            if (SetRace == null)
            {
                return null;
            }

            if (SetRace.StartingUnitsBlueprints.Count <= Index)
            {
                return null;
            }

            return SetRace.StartingUnitsBlueprints[Index];

        }

        public BuyUnitUI GetBuyUnitUiByIndex(int Index)
        {
            if (Index == 0)
            {
                return BuyBaseUnitUI;
            }
            else if (Index == 1)
            {
                return BuyRangedUnitUI;
            }
            else if (Index == 2)
            {
                return BuySpeedUnitUI;
            }
            else if (Index == 3)
            {
                return BuyTankUnitUI;
            }
            else if (Index == 4)
            {
                return BuyExtraUnitUI;
            }

            return null;

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
            BaseBuilding PlayerBuilding;
            if (PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.CustomGame || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Quickplay || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Ranked)
            {
                PlayerBuilding = BaseBuilding.TeamBuildings[PlayerManager.Instance.LocalPlayer.PlayerID];
            }
            else
            {
                PlayerBuilding = BaseBuilding.TeamBuildings[1];
            }
            PlayerHealthBar.maxValue = PlayerBuilding.MaxHealth;
            PlayerHealthBar.ChangeValue(PlayerBuilding.BuildingData.Health);

            PlayerXpBar.maxValue = PlayerBuilding.MaxXp;
            PlayerXpBar.ChangeValue(PlayerBuilding.BuildingData.Experience);

            PlayerArmorBar.maxValue = PlayerBuilding.MaxArmor;
            PlayerArmorBar.ChangeValue(PlayerBuilding.BuildingData.Armor);

            PlayerArmorBar.gameObject.SetActive(PlayerBuilding.MaxArmor > 0);

            BaseBuilding EnemyBuilding;
            if (PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.CustomGame || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Quickplay || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Ranked)
            {
                if (PlayerManager.Instance.LocalPlayer.PlayerID == 1)
                {
                    EnemyBuilding = BaseBuilding.TeamBuildings[2];
                }
                else
                {
                    EnemyBuilding = BaseBuilding.TeamBuildings[1];
                }
            }
            else
            {
                EnemyBuilding = BaseBuilding.TeamBuildings[2];
            }
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
            BaseBuilding PlayerBuilding;
            if (PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.CustomGame || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Quickplay || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Ranked)
            {
                PlayerBuilding = BaseBuilding.TeamBuildings[PlayerManager.Instance.LocalPlayer.PlayerID];
            }
            else
            {
                PlayerBuilding = BaseBuilding.TeamBuildings[1];
            }
            PopulationText.text = $"{PlayerBuilding.GetPopulation()} / {PlayerBuilding.BuildingData.MaxPopulation}";
        }

        public void UpdateCurrencyText()
        {
            CurrencyText.text = $"${CurrentCurrencyAmount}";
        }

        public void BuyUnit(BaseUnitBehaviour unit)
        {
            if (PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.CustomGame || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Quickplay || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Ranked)
            {
                BaseBuilding.TeamBuildings[PlayerManager.Instance.LocalPlayer.PlayerID].BuyUnit(unit);
            }
            else
            {
                BaseBuilding.TeamBuildings[1].BuyUnit(unit);
            }

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
}
