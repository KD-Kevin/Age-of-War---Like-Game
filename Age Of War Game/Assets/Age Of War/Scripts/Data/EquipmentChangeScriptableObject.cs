using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Data
{
    [CreateAssetMenu(fileName = "Tips", menuName = "AgeOfWar/Upgrades/EquipmentChange")]
    public class EquipmentChangeScriptableObject : ScriptableObject
    {
        public string EquipmentChangeDisplayName = "Enter Upgrade Name";
        public EquipmentUpgradeStatChanges Changes = new EquipmentUpgradeStatChanges();
        public List<EquipmentChangeScriptableObject> PossibleEquipmentChanges = new List<EquipmentChangeScriptableObject>();
        public List<ClassChangeScriptableObject> PossibleClassChanges = new List<ClassChangeScriptableObject>();
    }
}