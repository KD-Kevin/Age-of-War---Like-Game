#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;

namespace HeathenEngineering.SteamworksIntegration
{
    public struct WorkshopItemDataCreateStatus
    {
        public bool hasError;
        public string errorMessage;
        public PublishedFileId_t? ugcFileId;
        public CreateItemResult_t? createItemResult;
        public SubmitItemUpdateResult_t? submitItemUpdateResult;
    }
}
#endif