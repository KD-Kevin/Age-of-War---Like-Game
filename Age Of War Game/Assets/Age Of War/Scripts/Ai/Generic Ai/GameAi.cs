using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.AI
{
    /// <summary>
    /// The abstact class for each Ai. No difficulty is set as the class override will have each own specific difficulty
    /// </summary>
    public abstract class GameAi : MonoBehaviour
    {
        [SerializeField]
        protected string AiPrefabKey = "";
        public AiController Controller { get; set; }
        public ushort TeamID { get; set; }
        public bool Initialized { get; set; }
        public string AiKey { get { return AiPrefabKey; } }
        public AiDecisionTypes lastDecisionType { get; set; }
        public static List<GameAi> AllAi = new List<GameAi>();

        public virtual void Initialize()
        {
            if (!Initialized)
            {
                AllAi.Add(this);
                lastDecisionType = AiDecisionTypes.Initialize;
                Initialized = true;
            }
        }

        public virtual void OnDestroy()
        {
            if (AllAi.Contains(this))
            {
                AllAi.Remove(this);
            }
        }

        // This will change based on the Ai's Difficulty and Race, maybe for aggression type later on
        public abstract AiDecisionTypes MakeDecision();

        public static void AskAllAiForDecision()
        {
            foreach (GameAi ai in AllAi)
            {
                ai.MakeDecision();
            }
        }
    }

    public enum AiDecisionTypes
    {
        NoDecision, // Default value, different from idle as that is a deliberate decision and NoDecision is not
        Initialize, // Used for the beginning default value
        Idle, // Used when doing nothing is determined to be the best action or for difficulty settings, Generally more idle means worse ai (Unless it's saving for money??)

        // Creation
        BuildUnit,
        BuildTowerWeapon,
        BuildGroundWeapon,
        BuildWallWeapon,

        // Ability Usage
        UseSpecial,
        UseUltimate,

        // Upgrades
        BuyUnitUpgrade,
        BuyTowerWeaponUpgrade,
        BuyGroundWeaponWUpgrade,
        BuyWallWeaponUpgrade,
        BuyBaseUpgrade,

        // Class Change
        ChooseUnitClassChange,

        // Anything else
        Multiple, // Generally shouldb't be used, but more difficult Ai may use this
        RaceSpecificAction, // A Action that may only be possible for a specific race
        Other, // A decision that does not fit in any of the above criteria
    }
}
