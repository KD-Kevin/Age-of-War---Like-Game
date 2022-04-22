#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 

namespace HeathenEngineering.SteamworksIntegration
{
    public struct ConsumeOrder
    {
        public Steamworks.SteamItemDetails_t detail;
        public uint quantity;
    }
}
#endif