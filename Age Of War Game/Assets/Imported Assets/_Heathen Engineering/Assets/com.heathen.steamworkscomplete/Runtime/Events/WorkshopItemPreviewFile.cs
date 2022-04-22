#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;

namespace HeathenEngineering.SteamworksIntegration
{
    public struct WorkshopItemPreviewFile
    {
        public string source;
        /// <summary>
        /// YouTubeVideo and Sketchfab are not currently supported
        /// </summary>
        public EItemPreviewType type;
    }
}
#endif