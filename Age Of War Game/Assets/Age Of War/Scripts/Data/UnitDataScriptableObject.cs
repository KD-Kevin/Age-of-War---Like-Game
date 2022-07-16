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
        public StatChanges StatChanges = new StatChanges();

        public ArmorSprite NewHelmetSprite = null;
        public ArmorSprite NewTorsoSprite = null;
        public ArmorSprite NewLeggingSprite = null;
        public ArmorSprite NewShoeSprite = null;
        public MeleeWeapon NewWeapon = null;
        public RangedWeapon NewRangedWeapon = null; // WILL TAKE RANGED WEAPON IF BOTH THIS AND MELEE ARE NOT NULL
    }


    [System.Serializable]
    public class StatChanges
    {
        public int UnitCostChange = 0;

        public int MeleeDamageChange = 0;
        public float MeleeRangeChange = 0;

        public int ProjectileDamageChange = 0;
        public float ProjectileRangeChange = 0;

        /// <summary>
        /// base Experience given to the other unit when this is killed
        /// </summary>
        public int BaseXP = 0;
        /// <summary>
        /// How much this unit type gains on a kill (Base XP x ExperienceGainMultipler = Actual XP)
        /// </summary>
        public float ExperienceGainMultiplier = 1f;

        public int MaxHealthChange = 0;
        public bool HealEquivalentHealth = true;
        public int MaxArmorChange = 0;
        public bool HealEquivalentArmor = true;

        public float MovementSpeedChange = 0;

        public float AttackRateMultiplier = 1;
    }
}