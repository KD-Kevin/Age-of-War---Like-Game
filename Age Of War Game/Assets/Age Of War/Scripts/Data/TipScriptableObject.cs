using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Data
{
    [CreateAssetMenu(fileName = "Tips", menuName = "AgeOfWar/Tips/LoadingTips")]
    public class TipScriptableObject : ScriptableObject
    {
        public Localization TestLocalization;
        public List<string> Tips = new List<string>();

        public string GetRandomTip()
        {
            int RandomIndex = Random.Range(0, Tips.Count);

            return Tips[RandomIndex];
        }
    }
}
