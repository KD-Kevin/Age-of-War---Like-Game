using AgeOfWar.Core.Units;
using AgeOfWar.Networking;
using AgeOfWar.UI;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Core
{
    public class BaseBuilding : MonoBehaviour, IHealth, ITeam
    {
        [SerializeField]
        private int TeamBaseID = 0;
        [SerializeField]
        private Transform UnitSpawnTransform;

        [SerializeField]
        private LayerMask UnitRaycastLayer;
        [SerializeField]
        private List<Transform> TowerWeaponSpawnLocations = new List<Transform>(); // make sure there is enough slots for the max tower spawn points with the perks included for that race
        [SerializeField]
        private List<Transform> BackgroundWeaponSpawnLocations = new List<Transform>();
        [SerializeField]
        private List<Transform> FloorWeaponSpawnLocations = new List<Transform>(); // Usually Traps - Weapons that are not traps will need to take damage

        public BaseBuildingData BuildingData { get; set; }
        public int Team { get => TeamBaseID; set => SetTeam(value); }

        public int CurrentHealth { get => BuildingData.Health; set => BuildingData.Health = value; }
        public int MaxHealth { get => BuildingData.MaxHealth; set => BuildingData.MaxHealth = value; }
        public int StartingHealth { get => StartHealth; set => StartHealth = value; }
        public int CurrentArmor { get => BuildingData.Armor; set => BuildingData.Armor = value; }
        public int MaxArmor { get => BuildingData.MaxArmor; set => BuildingData.MaxArmor = value; }
        public int StartingArmor { get => StartArmor; set => StartArmor = value; }
        public int CurrentMagicArmor { get => BuildingData.MagicArmor; set => BuildingData.MagicArmor = value; }
        public int MaxMagicArmor { get => BuildingData.MaxMagicArmor; set => BuildingData.MaxMagicArmor = value; }
        public int StartingMagicArmor { get => StartMagicArmor; set => StartMagicArmor = value; }
        public int CurrentXp { get => BuildingData.Experience; set => BuildingData.Experience = value; }
        public int MaxXp { get => BuildingData.MaxExperience; set => BuildingData.MaxExperience = value; }
        public int AutoHeal { get => BuildingData.AutoHealAmount; set => BuildingData.AutoHealAmount = value; }
        public int AutoRepair { get => BuildingData.AutoRepairAmount; set => BuildingData.AutoRepairAmount = value; }
        public int AutoRestore { get => BuildingData.AutoRestoreAmount; set => BuildingData.AutoRestoreAmount = value; }

        public float DamgeMitigationMultiplier { get => BuildingData.DamageMitigationMultiplier; set => BuildingData.DamageMitigationMultiplier = value; }

        public static Dictionary<int, BaseBuilding> TeamBuildings = new Dictionary<int, BaseBuilding>();

        protected int StartHealth = 0;
        protected int StartArmor = 0;
        protected int StartMagicArmor = 0;
        protected int FrameCounter = 0;
        protected float RepairHealRestoreTimer = 0;
        protected RaycastHit2D[] RayHits = new RaycastHit2D[0];
        [HideInInspector]
        public List<BaseUnitBehaviour> BuyUnitBuffer = new List<BaseUnitBehaviour>();
        [HideInInspector]
        public List<int> HoldPopulationSpot = new List<int>();
        public List<BuildOrder> BuildOrders = new List<BuildOrder>();

        private void Awake()
        {
            if (TeamBuildings.ContainsKey(TeamBaseID))
            {
                Debug.LogError($"Two Bases of the same team exist {TeamBaseID}");
            }
            else
            {
                TeamBuildings.Add(TeamBaseID, this);
            }

            SetTeam(TeamBaseID);
            BuildingData = new BaseBuildingData();
        }

        public static void UpdateBases()
        {
            foreach (BaseBuilding Base in TeamBuildings.Values)
            {
                Base.BaseUpdate();
            }
        }

        private void BaseUpdate()
        {
            if (BuyUnitBuffer.Count > 0)
            {
                // Check to see if we can buy unit
                RayHits = Physics2D.RaycastAll(UnitSpawnTransform.position, UnitSpawnTransform.forward, 3, UnitRaycastLayer);
                if (RayHits.Length == 0)
                {
                    SpawnUnit(BuyUnitBuffer[0]);
                    BuyUnitBuffer.RemoveAt(0);
                }
            }

            if (AutoHeal != 0 || AutoRepair != 0 || AutoRestore != 0)
            {
                if (CurrentHealth < MaxHealth || CurrentArmor < MaxArmor || CurrentMagicArmor < MaxMagicArmor)
                {
                    RepairHealRestoreTimer += LockstepManager.Instance.StepTime;
                    if (RepairHealRestoreTimer > 1f)
                    {
                        RepairHealRestoreTimer = 0;
                        Heal(AutoHeal);
                        Repair(AutoRepair);
                        Restore(AutoRestore);
                    }
                }
            }

            for (int buildOrderIndex = BuildOrders.Count - 1; buildOrderIndex >= 0; buildOrderIndex--)
            {
                BuildOrders[buildOrderIndex].BuildTime -= LockstepManager.Instance.StepTime;
                if (BuildOrders[buildOrderIndex].BuildTime <= 0)
                {
                    BuyUnit(BuildOrders[buildOrderIndex].Order);
                    PlayerUiManager.Instance.UpdatePopulationText();
                    PlayerUiManager.Instance.UpdateCurrencyText();
                    BuildOrders.RemoveAt(buildOrderIndex);
                }
            }
        }

        public void BuyUnit(BaseUnitBehaviour UnitPrefab)
        {
            BuyUnitBuffer.Add(UnitPrefab);
        }

        public void SpawnUnit(BaseUnitBehaviour UnitPrefab)
        {
            BaseUnitBehaviour.SpawnUnit(UnitPrefab, UnitSpawnTransform.position).SetTeam(TeamBaseID);
        }

        public void SetNewData(BaseBuildingData Data)
        {
            BuildingData = Data;
        }

        public static void AddBuildOrder(BuildOrder buildOrder)
        {
            if (buildOrder.OwnerID == 0 || buildOrder.OwnerID > 2)
            {
                return;
            }
            TeamBuildings[buildOrder.OwnerID].BuildOrders.Add(buildOrder);
        }

        public int GetPopulation()
        {
            if (!BaseUnitBehaviour.AllActiveTeamUnits.ContainsKey(TeamBaseID))
            {
                return BuyUnitBuffer.Count + HoldPopulationSpot.Count;
            }

            return BuyUnitBuffer.Count + BaseUnitBehaviour.AllActiveTeamUnits[TeamBaseID].Count + HoldPopulationSpot.Count;
        }

        #region IHealth Interface

        public virtual void Damage(int DamageAmount, string DamageReason, DamageTypes DamageType)
        {
            // Multipliers here

            // Defenses Here
            DamageAmount = Mathf.FloorToInt(BuildingData.DamageMitigationMultiplier * DamageAmount);
            int Remainder = DamageAmount;
            if (DamageType == DamageTypes.Physical)
            {
                // Modifiers Here
                if (CurrentArmor > 0)
                {
                    if (DamageAmount > 10)
                    {
                        DamageAmount -= 10;
                    }
                    else
                    {
                        DamageAmount /= 2;
                    }
                }
                Remainder = DamageAmount - CurrentArmor;
                CurrentArmor -= DamageAmount;
                if (CurrentArmor < 0)
                {
                    CurrentArmor = 0;
                }
            }
            else if (DamageType == DamageTypes.Magical)
            {
                if (CurrentMagicArmor > 0)
                {
                    if (DamageAmount > 10)
                    {
                        DamageAmount -= 10;
                    }
                    else
                    {
                        DamageAmount /= 2;
                    }
                }
                Remainder = DamageAmount - CurrentMagicArmor;
                CurrentMagicArmor -= DamageAmount;
                if (CurrentMagicArmor < 0)
                {
                    CurrentMagicArmor = 0;
                }
            }

            // Damage Here
            if (Remainder > 0)
            {
                CurrentHealth -= Remainder;

                if (CurrentHealth < 0)
                {
                    CurrentHealth = 0;
                    DieOff(DamageReason);
                }
            }
        }

        public virtual void DieOff(string DeathReason)
        {
            // Game Over - You Lose
        }

        public virtual void Heal(int HealAmount)
        {
            // Modifiers Here

            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += HealAmount;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        public virtual void HealAndRepair(int RepairAmount)
        {
            // Modifiers Here

            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += RepairAmount;
            int Remainder = 0;
            if (CurrentHealth > MaxHealth)
            {
                Remainder = CurrentHealth - MaxHealth;
                CurrentHealth = MaxHealth;
            }

            if (Remainder > 0)
            {
                CurrentArmor += Remainder;
                if (CurrentArmor > MaxArmor)
                {
                    CurrentArmor = MaxArmor;
                }
            }
        }

        public virtual void HealAndRestore(int RestoreAmount)
        {
            // Modifiers Here

            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += RestoreAmount;
            int Remainder = 0;
            if (CurrentHealth > MaxHealth)
            {
                Remainder = CurrentHealth - MaxHealth;
                CurrentHealth = MaxHealth;
            }

            if (Remainder > 0)
            {
                CurrentMagicArmor += Remainder;
                if (CurrentMagicArmor > MaxMagicArmor)
                {
                    CurrentMagicArmor = MaxMagicArmor;
                }
            }
        }

        public virtual void RepairAndRestore(int RepairAmount)
        {
            // Modifiers Here

            CurrentArmor += RepairAmount;
            int Remainder = 0;
            if (CurrentArmor > MaxArmor)
            {
                Remainder = CurrentArmor - MaxArmor;
                CurrentArmor = MaxArmor;
            }

            if (Remainder > 0)
            {
                CurrentMagicArmor += Remainder;
                if (CurrentMagicArmor > MaxMagicArmor)
                {
                    CurrentMagicArmor = MaxMagicArmor;
                }
            }
        }

        public virtual void RestoreAndRepair(int RestoreAmount)
        {
            // Modifiers Here

            CurrentMagicArmor += RestoreAmount;
            int Remainder = 0;
            if (CurrentMagicArmor > MaxMagicArmor)
            {
                Remainder = CurrentMagicArmor - MaxMagicArmor;
                CurrentMagicArmor = MaxMagicArmor;
            }

            if (Remainder > 0)
            {
                CurrentArmor += Remainder;
                if (CurrentArmor > MaxArmor)
                {
                    CurrentArmor = MaxArmor;
                }
            }
        }

        public virtual void HealRepairAndRestore(int Amount)
        {
            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += Amount;
            int Remainder = 0;
            if (CurrentHealth > MaxHealth)
            {
                Remainder = CurrentHealth - MaxHealth;
                CurrentHealth = MaxHealth;
            }

            if (Remainder > 0)
            {
                CurrentArmor += Remainder;
                if (CurrentArmor > MaxArmor)
                {
                    Remainder = CurrentArmor - MaxArmor;
                    CurrentArmor = MaxArmor;
                }
                else
                {
                    Remainder = 0;
                }
            }

            if (Remainder > 0)
            {
                CurrentMagicArmor += Remainder;
                if (CurrentMagicArmor > MaxMagicArmor)
                {
                    Remainder = CurrentMagicArmor - MaxMagicArmor;
                    CurrentMagicArmor = MaxMagicArmor;
                }
                else
                {
                    Remainder = 0;
                }
            }
        }

        public virtual void HealRestoreAndRepair(int Amount)
        {
            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += Amount;
            int Remainder = 0;
            if (CurrentHealth > MaxHealth)
            {
                Remainder = CurrentHealth - MaxHealth;
                CurrentHealth = MaxHealth;
            }

            if (Remainder > 0)
            {
                CurrentMagicArmor += Remainder;
                if (CurrentMagicArmor > MaxMagicArmor)
                {
                    Remainder = CurrentMagicArmor - MaxMagicArmor;
                    CurrentMagicArmor = MaxMagicArmor;
                }
                else
                {
                    Remainder = 0;
                }
            }

            if (Remainder > 0)
            {
                CurrentArmor += Remainder;
                if (CurrentArmor > MaxArmor)
                {
                    Remainder = CurrentArmor - MaxArmor;
                    CurrentArmor = MaxArmor;
                }
                else
                {
                    Remainder = 0;
                }
            }
        }

        public virtual void RecieveExperience(int Experience)
        {
            if (CurrentXp == MaxXp)
            {
                return;
            }

            CurrentXp += Experience;
            if (CurrentXp > MaxXp)
            {
                CurrentXp = MaxXp;
            }
        }

        public virtual void Initialize()
        {
            CurrentArmor = StartArmor;
            CurrentHealth = StartHealth;
        }

        public virtual void Repair(int RepairAmount)
        {
            // Modifiers Here

            CurrentArmor += RepairAmount;
            if (CurrentArmor > MaxArmor)
            {
                CurrentArmor = MaxArmor;
            }
        }

        public virtual void Restore(int RestoreAmount)
        {
            // Modifiers Here

            CurrentMagicArmor += RestoreAmount;
            if (CurrentMagicArmor > MaxMagicArmor)
            {
                CurrentMagicArmor = MaxMagicArmor;
            }
        }

        public void SetTeam(int TeamID)
        {
            TeamBaseID = TeamID;

            // Maybe do other stuff? should be called early
        }

        #endregion
    }

    [System.Serializable]
    public class BaseBuildingData
    {
        public int Health = 1000;
        public int MaxHealth = 1000;
        public int Armor = 0;
        public int MaxArmor = 0;
        public int MagicArmor = 0;
        public int MaxMagicArmor = 0;
        public int AutoRepairAmount = 0;
        public int AutoHealAmount = 0;
        public int AutoRestoreAmount = 0;
        public float DamageMitigationMultiplier = 1;

        public int Experience = 0;
        public int MaxExperience = 1000;

        public int MaxPopulation = 5;
    }
}
