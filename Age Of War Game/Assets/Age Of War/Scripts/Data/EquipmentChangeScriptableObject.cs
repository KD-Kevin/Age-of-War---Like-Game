using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Data
{
    [CreateAssetMenu(fileName = "Tips", menuName = "AgeOfWar/Upgrades/EquipmentChange")]
    public class EquipmentChangeScriptableObject : ScriptableObject
    {
        [TextArea(1, 2)]
        public string EquipmentChangeDisplayName = "Enter Upgrade Name";
        [TextArea(3,8)]
        public string EquipmentChangeDisplayDescription = "Enter Upgrade Discription";
        public Sprite ShopDisplay;
        public int EquipmentCost = 0;
        public EquipmentUpgradeStatChanges Changes = new EquipmentUpgradeStatChanges();
        public List<EquipmentChangeScriptableObject> PossibleEquipmentChanges = new List<EquipmentChangeScriptableObject>();
        public List<ClassChangeScriptableObject> PossibleClassChanges = new List<ClassChangeScriptableObject>();
        public List<int> AstheticChangeInIDs = new List<int>(); // List of Asthetics that get turned "ON" when the Equipment is added
        public List<int> AstheticChangeOutIDs = new List<int>(); // List of Asthetics that get turned "OFF" when the Equipment is added
    }
}