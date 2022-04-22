#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 

namespace HeathenEngineering.SteamworksIntegration.Enums
{
    /// <summary>
    /// The type applied to a Steam Inventory Item
    /// </summary>
    public enum InventoryItemType
    {
        item,
        bundle,
        generator,
        playtimegenerator,
        tag_generator,
    }
}
#endif