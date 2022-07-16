using UnityEngine;
using AgeOfWar.Core.Units;

namespace AgeOfWar.Data
{
    [CreateAssetMenu(fileName = "Tips", menuName = "AgeOfWar/Upgrades/ClassChange")]
    public class ClassChangeScriptableObject : ScriptableObject
    {
        public string ClassChangeDisplayName = "Enter Upgrade Name";
        public BaseUnitBehaviour NewUnitType;
    }
}
