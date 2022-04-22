#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct ItemInstanceChangeRecord
    {
        public SteamItemInstanceID_t instance;
        public bool added;
        public bool removed;
        public bool changed;
        public ushort quantityBefore;
        public ushort quantityAfter;
        public int QuantityChange => quantityAfter - quantityBefore;
    }
}
#endif