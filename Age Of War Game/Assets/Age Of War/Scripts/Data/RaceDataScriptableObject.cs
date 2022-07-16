using System.Collections.Generic;
using UnityEngine;
using AgeOfWar.Core.Units;
using AgeOfWar.Core;
using AgeOfWar.AI;

namespace AgeOfWar.Data
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "AgeOfWar/Species/RaceData")]
    public class RaceDataScriptableObject : ScriptableObject
    {
        [Header("Basic Details")]
        public string DisplayName;
        [TextArea]
        public string DisplayDescription;
        [TextArea]
        public string DisplayPlaystyleDescription;
        public string CampainSceneName;
        public Sprite DisplaySprite;
        public RaceTypes RaceType;

        [Header("UI For Race")]
        public GameObject RaceSpecificUiPrefab;

        [Header("Base Building")]
        public BaseBuilding BaseBlueprint;

        [Header("Ultimates")]
        public List<Ultimate> Ultimates = new List<Ultimate>(); // Ultimates that the race can use - Sort them be cost

        [Header("Perks")]
        public List<Perk> PossiblePerks = new List<Perk>();

        [Header("Units")]
        public List<BaseUnitBehaviour> StartingUnitsBlueprints = new List<BaseUnitBehaviour>(); // Should not be more then 4 they design is typically 4 per race (Some exceptions can include 5)

        [Header("Weapons")]
        public List<TowerWeapon> StartingTowerWeaponBlueprints = new List<TowerWeapon>();
        public List<TowerWeapon> UnlockableTowerWeaponBlueprints = new List<TowerWeapon>();

        public List<FloorWeapon> StartingFloorWeaponBlueprints = new List<FloorWeapon>();
        public List<FloorWeapon> UnlockableFloorWeaponBlueprints = new List<FloorWeapon>();

        public List<BackgroundWeapon> StartingBackgroundWeaponBlueprints = new List<BackgroundWeapon>();
        public List<BackgroundWeapon> UnlockableBackgroundWeaponBlueprints = new List<BackgroundWeapon>();

        [Header("Passive")]
        public RacePassive Passive;

        // Ai Difficulties - If these are not set, it will grab from the PlayerManager
        [Header("Ai")]
        public GameAiScriptableObject VeryEasyAi;
        public GameAiScriptableObject EasyAi;
        public GameAiScriptableObject NormalAi;
        public GameAiScriptableObject HardAi;
        public GameAiScriptableObject InsaneAi;
        public GameAiScriptableObject YouWontAi;

        public int GetPerkIndex(Perk WantedPerkInfo)
        {
            if (WantedPerkInfo == null)
            {
                return -1;
            }

            for (int index = 0; index < PossiblePerks.Count; index++)
            {
                if (PossiblePerks[index].DisplayName == WantedPerkInfo.DisplayName)
                {
                    return index;
                }
            }

            return -1;
        }
    }

    public enum RaceTypes
    {
        Unknown,
        Human,
        Elven,
        Dwarfes,
        Goblin,
        CrazyFarmer,
        Froggen,
    }
}