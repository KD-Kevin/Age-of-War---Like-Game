using System.Collections.Generic;
using UnityEngine;
using AgeOfWar.Core.Units;
using AgeOfWar.Data;
using AgeOfWar.Networking;
using AgeOfWar.Core;
using AgeOfWar.UI;

namespace AgeOfWar.AI
{
    /// <summary>
    /// The Controller the Ai used to play the game. All the controls the Ai will have access to will be here
    /// </summary>
    public class AiController : MonoBehaviour
    {
        #region Run and Initialization / Deinitialization

        public List<AiBuyUnitData> UnitPoolData = new List<AiBuyUnitData>();
        public static List<AiController> Controllers = new List<AiController>();
        public GameAi CurrentAi { get; set; }

        private void Awake()
        {
            Controllers.Add(this); ;
        }

        /// <summary>
        /// Loads the data needed for the AI at
        /// </summary>
        public bool Loaded { get; private set; }
        public void LoadController(RaceDataScriptableObject RaceData, GameAiScriptableObject AiData, ushort Team)
        {
            if (!Loaded)
            {
                int Index = 0;
                foreach (BaseUnitBehaviour Units in RaceData.StartingUnitsBlueprints)
                {
                    AiBuyUnitData Data = new AiBuyUnitData(Units, Index);
                    Index++;
                }

                CurrentMoney = AiData.StartingMoney;
                CurrentAi = Instantiate(AiData.AiPrefab, transform);
                CurrentAi.Controller = this;
                CurrentAi.TeamID = Team;
                CurrentAi.Initialize();

                Loaded = true;
            }
        }

        /// <summary>
        /// Unloads the Ai, Makes it uncapable to doing actions
        /// </summary>
        public void UnLoad()
        {
            if (Loaded)
            {
                UnitPoolData.Clear();
                Loaded = false;
                Destroy(CurrentAi.gameObject);
                CurrentAi = null;
            }
        }

        public static void UpdateAiControllers()
        {
            foreach (AiController controller in Controllers)
            {
                controller.ControllerUpdate();
            }
        }

        private void ControllerUpdate()
        {
            if (!Loaded)
            {
                return;
            }

            foreach (AiBuyUnitData Data in UnitPoolData)
            {
                if (Data.CooldownTimer > 0)
                {
                    Data.CooldownTimer -= LockstepManager.Instance.HalfStepTime;
                }
            }
        }

        #endregion

        #region Actions

        public void BuyUnit(AiBuyUnitData Data)
        {
            if (CanAffordUnit(Data))
            {
                if (Data.CooldownTimer > 0)
                {
                    return;
                }

                BaseBuilding HomeBase = BaseBuilding.TeamBuildings[CurrentAi.TeamID];
                if (HomeBase.GetPopulation() >= HomeBase.BuildingData.MaxPopulation)
                {
                    return;
                }

                Data.CooldownTimer = Data.Prefab.GetCooldown();
                //if (PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.CustomGame || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Quickplay || PlayerManager.Instance.CurrentMatchData.PlayMode == PlayModes.Ranked)
                //{
                // Send of Action to lockstep
                BuyUnitAction NewAction = new BuyUnitAction(Data.BuyIndex);
                NewAction.OwningPlayer = CurrentAi.TeamID;
                LockstepManager.Instance.AddAiAction(NewAction);
                return;
            }
        }

        #endregion

        #region State Conditions

        public int CurrentMoney { get; private set; }

        public bool CanAffordUnit(AiBuyUnitData UnitData)
        {
            return CurrentMoney >= UnitData.Prefab.GetBuildCost();
        }

        #endregion
    }

    public class AiBuyUnitData
    {
        public BaseUnitBehaviour Prefab;
        public float CooldownTimer = 0;
        public int BuyIndex;

        public AiBuyUnitData(BaseUnitBehaviour prefab, int Index)
        {
            Prefab = prefab;
            BuyIndex = Index;
        }
    }
}