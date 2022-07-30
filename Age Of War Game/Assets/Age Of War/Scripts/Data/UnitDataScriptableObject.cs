using AgeOfWar.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Data
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "AgeOfWar/Upgrades/UnitData")]
    public class UnitDataScriptableObject : ScriptableObject
    {
        public UnitData Data = new UnitData();
    }

    [System.Serializable]
    public class UnitData
    {
        public List<EquipmentChangeScriptableObject> PossibleEquipmentChanges = new List<EquipmentChangeScriptableObject>();

        public UnitData Clone()
        {
            UnitData Data = new UnitData();

            Data.PossibleEquipmentChanges.AddRange(PossibleEquipmentChanges);

            return Data;
        }
    }

    [System.Serializable]
    public class EquipmentUpgradeStatChanges
    {
        // Basic Stat Changes
        public StatChanges StatChanges = new StatChanges();

        // If i think of anything Equipment specific change, add them here
    }


    [System.Serializable]
    public struct StatChanges
    {
        [Header("Unit Cost Changes")]
        public int UnitCostChange;

        [Header("Experience, Build Time, Cooldown Changes")]
        /// <summary>
        /// base Experience given to the other unit when this is killed
        /// </summary>
        public int OnKilledXPGiven;
        /// <summary>
        /// How much this unit type gains on a kill (Base XP x ExperienceGainMultipler = Actual XP)
        /// </summary>
        public float ExperienceGainMultiplier;
        public float CooldownChange;
        public float BuildTimeChange;

        [Header("Damage Changes")]
        public int PhysicalDamageChange;
        public int MagicDamageChange;
        public int TrueDamageChange;
        public float RangeChange;

        // Health, Armor, Magic Armor and related
        [Header("Health, Armor, etc Changes")]
        public int AutoHealAmountChange;
        public int MaxHealthChange;
        public bool HealEquivalentHealth;
        public int AutoRepairAmountChange;
        public int MaxArmorChange;
        public bool RepairEquivalentArmor;
        public int AutoRestoreAmountChange;
        public int MaxMagicArmorChange;
        public bool RestoreEquivalentArmor;
        public float DamageMitigationChange; // Flat Mitigation
        public float DamageMitigationMultiplier; // Rate Mitigation

        [Header("Movement Changes")]
        public float MovementSpeedChange;
        public float MovementSpeedMultiplier;

        [Header("Attack Rate Changes")]
        public float AttackRateMultiplier;

        public StatChanges(int unitCostChange = 0, int physicalDamageChange = 0, int magicDamageChange = 0, int trueDamageChange = 0, float rangeChange = 0, int onKilledXPGiven = 0, float experienceGainMultiplier = 1f,
            float cooldownChange = 0, float buildTimeChange = 0,
            int autoHealAmountChange = 0, int autoRepairAmountChange = 0, int autoRestoreAmountChange = 0, int maxHealthChange = 0, int maxArmorChange = 0, int maxMagicArmorChange = 0, float damageMitigationChange = 0, float damageMitigationMultiplier = 0,
            float movementSpeedChange = 0, float movementSpeedMultiplier = 1, float attackRateMultiplier = 1, bool healHealthChange = true, bool repairArmorChange = true, bool restoreMagicArmorChange = true)
        {
            UnitCostChange = unitCostChange;

            PhysicalDamageChange = physicalDamageChange;
            MagicDamageChange = magicDamageChange;
            TrueDamageChange = trueDamageChange;
            RangeChange = rangeChange;

            OnKilledXPGiven = onKilledXPGiven;
            ExperienceGainMultiplier = experienceGainMultiplier;
            CooldownChange = cooldownChange;
            BuildTimeChange = buildTimeChange;

            AutoHealAmountChange = autoHealAmountChange;
            MaxHealthChange = maxHealthChange;
            HealEquivalentHealth = healHealthChange;
            AutoRepairAmountChange = autoRepairAmountChange;
            MaxArmorChange = maxArmorChange;
            RepairEquivalentArmor = repairArmorChange;
            AutoRestoreAmountChange = autoRestoreAmountChange;
            MaxMagicArmorChange = maxMagicArmorChange;
            RestoreEquivalentArmor = restoreMagicArmorChange;
            DamageMitigationChange = damageMitigationChange;
            DamageMitigationMultiplier = damageMitigationMultiplier;

            MovementSpeedChange = movementSpeedChange;
            MovementSpeedMultiplier = movementSpeedMultiplier;

            AttackRateMultiplier = attackRateMultiplier;
    }

        public static StatChanges operator +(StatChanges a, StatChanges b)
        {
            StatChanges AddedChanges = new StatChanges();

            AddedChanges.UnitCostChange = a.UnitCostChange + b.UnitCostChange;

            AddedChanges.PhysicalDamageChange = a.PhysicalDamageChange + b.PhysicalDamageChange;
            AddedChanges.MagicDamageChange = a.MagicDamageChange + b.MagicDamageChange;
            AddedChanges.TrueDamageChange = a.TrueDamageChange + b.TrueDamageChange;
            AddedChanges.RangeChange = a.RangeChange + b.RangeChange;

            AddedChanges.OnKilledXPGiven = a.OnKilledXPGiven + b.OnKilledXPGiven;
            AddedChanges.ExperienceGainMultiplier = a.ExperienceGainMultiplier * b.ExperienceGainMultiplier;
            AddedChanges.CooldownChange = a.CooldownChange * b.CooldownChange;
            AddedChanges.BuildTimeChange = a.BuildTimeChange * b.BuildTimeChange;

            AddedChanges.MaxHealthChange = a.MaxHealthChange + b.MaxHealthChange;
            AddedChanges.MaxArmorChange = a.MaxArmorChange + b.MaxArmorChange;
            AddedChanges.MaxMagicArmorChange = a.MaxMagicArmorChange + b.MaxMagicArmorChange;
            AddedChanges.AutoHealAmountChange = a.AutoHealAmountChange + b.AutoHealAmountChange;
            AddedChanges.AutoRepairAmountChange = a.AutoRepairAmountChange + b.AutoRepairAmountChange;
            AddedChanges.AutoRestoreAmountChange = a.AutoRestoreAmountChange + b.AutoRestoreAmountChange;
            AddedChanges.DamageMitigationChange = a.DamageMitigationChange + b.DamageMitigationChange;
            AddedChanges.DamageMitigationMultiplier = a.DamageMitigationMultiplier * b.DamageMitigationMultiplier;
            AddedChanges.HealEquivalentHealth = false;
            AddedChanges.RepairEquivalentArmor = false;
            AddedChanges.RestoreEquivalentArmor = false;

            AddedChanges.MovementSpeedChange = a.MovementSpeedChange + b.MovementSpeedChange;
            AddedChanges.AttackRateMultiplier = a.AttackRateMultiplier * b.AttackRateMultiplier;
            AddedChanges.MovementSpeedMultiplier = a.MovementSpeedMultiplier * b.MovementSpeedMultiplier;

            return AddedChanges;
        }
    }
}