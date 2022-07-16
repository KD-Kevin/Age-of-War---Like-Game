using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Data
{
    [CreateAssetMenu(fileName = "TipManager", menuName = "AgeOfWar/Tips/TipManager")]
    public class TipLocalizationManager : ScriptableObject
    {
        public List<TipScriptableObject> Localizations = new List<TipScriptableObject>();

        public TipScriptableObject GetLocalizations(Localization TestLocalization = Localization.English)
        {
            foreach (TipScriptableObject localization in Localizations)
            {
                if (localization.TestLocalization == TestLocalization)
                {
                    return localization;
                }
            }

            return null;
        }
    }



    public enum Localization
    {
        English,
        French,
        Number,
    }
}