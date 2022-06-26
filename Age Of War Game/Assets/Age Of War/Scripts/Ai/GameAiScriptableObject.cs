using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AiData", menuName = "AgeOfWar/Ai/AiData")]
public class GameAiScriptableObject : ScriptableObject
{
    public int StartingMoney;
    public GameAi AiPrefab;

}
