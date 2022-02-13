using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tips", menuName = "AgeOfWar/Upgrades/ClassChange")]
public class ClassChangeScriptableObject : ScriptableObject
{
    public string ClassChangeDisplayName = "Enter Upgrade Name";
    public BaseUnitBehaviour NewUnitType;
}
