using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Core
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "AgeOfWar/Perks/PerkData")]
    public class Perk : ScriptableObject
    {
        public string DisplayName;
        public string DisplayDescription;
        public Sprite DisplaySprite;
        public List<PerkTypes> PerkCatagories = new List<PerkTypes>();

        public PerkChanges BasicChanges = new PerkChanges();

        // Special Effects this perk does at the start of the game
        public virtual void OnStartEffects()
        {

        }

        // Special Effects this perk does on every update call
        public virtual void OnUpdateEffects()
        {

        }

        public bool IsPerkType(PerkTypes Type)
        {
            foreach (PerkTypes type in PerkCatagories)
            {
                if (type == Type || type == PerkTypes.All)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [System.Serializable]
    public class PerkChanges
    {
        [Header("Player Monetary Effects")]
        public int FlatMoneyIncreaseAtStart = 0;
        public float MoneyGainMultiplier = 1f;
        public float WeaponPriceMultiplier = 1f;
        public float UnitPriceMultiplier = 1f;

        [Header("Player Experience Effects")]
        public int FlatExperienceIncreaseAtStart = 0;
        public float ExperienceGainMultiplier = 1f;

        [Header("Player Ultimate Effects")]
        [Range(0, 100)]
        public float FlatUltimateChargePercentIncreaseAtStart = 0;
        public float UltimateGainMultiplier = 1f;

        [Header("Base Health / Armor Effects")]
        public int BaseBuildingMaxHealthChange = 0;
        public int BaseBuildingMaxArmorChange = 0;
        public int BaseBuildingMaxMagicArmorChange = 0;
        public int BaseBuildingAutoHealChange = 0; // Health
        public int BaseBuildingAutoRepairChange = 0; // Armor
        public int BaseBuildingMAutoRestoreChange = 0; // Magic Armor

        [Header("Base Weapon Effects")]
        public int MaxTowerWeaponSlotsChange = 0;
        public int TowerWeaponDamageChange = 0;
        public int TowerWeaponRangeChange = 0;
        public float WeaponBuildTimeMultiplier = 1f;

        [Header("Unit Cost Effects")]
        public int AllUnitCostChange = 0;
        public List<int> PerUnitCostChange = new List<int>()
    {
        0,0,0,0
    };

        [Header("Unit Melee Effects")]
        public int AllUnitMeleeDamageChange = 0;
        public List<int> PerUnitMeleeDamageChange = new List<int>()
    {
        0,0,0,0
    };
        public float AllUnitMeleeRangeChange = 0;
        public List<int> PerUnitMeleeRangeChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitMeleeMagicDamageChange = 0;
        public List<int> PerUnitMeleeMagicDamageChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitMeleeTrueDamageChange = 0;
        public List<int> PerUnitMeleeTrueDamageChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitMeleeLifestealChange = 0;
        public List<int> PerUnitMeleeLifestealChange = new List<int>()
    {
        0,0,0,0
    };

        [Header("Unit Ranged Effects")]
        public int AllUnitProjectileDamageChange = 0;
        public List<int> PerUnitProjectileDamageChange = new List<int>()
    {
        0,0,0,0
    };
        public float AllUnitProjectileRangeChange = 0;
        public List<int> PerUnitProjectileRangeChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitProjectileMagicDamageChange = 0;
        public List<int> PerUnitProjectileMagicDamageChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitProjectileTrueDamageChange = 0;
        public List<int> PerUnitProjectileTrueDamageChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitProjectileLifestealChange = 0;
        public List<int> PerUnitProjectileLifestealChange = new List<int>()
    {
        0,0,0,0
    };

        [Header("Unit Experience Effects")]
        public int AllUnitBaseExperienceChange = 0; // Buff for the other player
        public List<int> PerUnitExperienceGivenWhenKilledChange = new List<int>()
    {
        0,0,0,0
    };

        [Header("Unit Health Effects")]
        public int AllUnitMaxHealthChange = 0;
        public List<int> PerUnitMaxHealthChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitAutoHealChange = 0;
        public List<int> PerUnitAutoHealChange = new List<int>()
    {
        0,0,0,0
    };

        [Header("Unit Armor Effects")]
        public int AllUnitMaxArmorChange = 0;
        public List<int> PerUnitMaxArmorChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitAutoRepairChange = 0;
        public List<int> PerUnitAutoRepairChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitMaxMagicArmorChange = 0;
        public List<int> PerUnitMaxMagicArmorChange = new List<int>()
    {
        0,0,0,0
    };
        public int AllUnitAutoRestoreChange = 0;
        public List<int> PerUnitAutoRestoreChange = new List<int>()
    {
        0,0,0,0
    };
        // Absolute Mitigation Percentage Change - Damage Mitigation Capped at "50% / -50%"
        public float AllUnitDamageMitigationMultiplierChange = 0;
        public List<int> PerUnitDamageMitigationMultiplierChange = new List<int>()
    {
        0,0,0,0
    };

        [Header("Unit Movement Effects")]
        public float AllUnitMovementSpeedChange = 0f;
        public List<float> PerUnitMovementSpeedChange = new List<float>()
    {
        0f,0f,0f,0f
    };

        [Header("Unit Cooldown Effects")]
        public float AllUnitCooldownMultiplier = 1f;
        public List<float> PerUnitCooldownMultiplier = new List<float>()
    {
        1f,1f,1f,1f
    };

        [Header("Unit Build Time Effects")]
        public float AllUnitBuildTimeMultiplier = 1f;
        public List<float> PerUnitBuildTimeMultiplier = new List<float>()
    {
        1f,1f,1f,1f
    };
    }

    public enum PerkTypes
    {
        All,
        Economy,
        Weapon,
        Unit,
        Base,
        Ultimate,
        Other,
    }
}
