#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;

namespace HeathenEngineering.SteamworksIntegration
{
    public struct ExchangeEntry
    {
        public SteamItemInstanceID_t instance;
        public uint quantity;
    }

}
#endif