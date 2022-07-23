using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Data
{
    [CreateAssetMenu(fileName = "Tips", menuName = "AgeOfWar/Upgrades/EquipmentChange")]
    public class EquipmentChangeScriptableObject : ScriptableObject
    {
        public Sprite ShopDisplay;
        public List<int> AstheticChangeInIDs = new List<int>(); // List of Asthetics that get turned "ON" when the Equipment is added
        public List<int> AstheticChangeOutIDs = new List<int>(); // List of Asthetics that get turned "OFF" when the Equipment is added
        public string EquipmentChangeDisplayName = "Enter Upgrade Name";
        public EquipmentUpgradeStatChanges Changes = new EquipmentUpgradeStatChanges();
        public List<EquipmentChangeScriptableObject> PossibleEquipmentChanges = new List<EquipmentChangeScriptableObject>();
        public List<ClassChangeScriptableObject> PossibleClassChanges = new List<ClassChangeScriptableObject>();
    }
}