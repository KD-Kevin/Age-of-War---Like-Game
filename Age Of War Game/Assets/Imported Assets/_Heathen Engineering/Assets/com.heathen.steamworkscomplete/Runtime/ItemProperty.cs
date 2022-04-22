#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct ItemProperty
    {
        public string key;
        public string value;
    }
}
#endif