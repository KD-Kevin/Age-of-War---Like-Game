using UnityEngine;

namespace AgeOfWar.AI
{
    [CreateAssetMenu(fileName = "AiData", menuName = "AgeOfWar/Ai/AiData")]
    public class GameAiScriptableObject : ScriptableObject
    {
        public int StartingMoney;
        public GameAi AiPrefab;
    }
}
