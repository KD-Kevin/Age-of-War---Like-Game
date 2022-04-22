#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Structure of the player entry data returned by the <see cref="GameServerBrowserManager.PlayerDetails(GameServerBrowserEntery, Action{GameServerBrowserEntery, bool})"/> method
    /// </summary>
    [Serializable]
    public class ServerPlayerEntry
    {
        public string name;
        public int score;
        public TimeSpan timePlayed;
    }
}
#endif