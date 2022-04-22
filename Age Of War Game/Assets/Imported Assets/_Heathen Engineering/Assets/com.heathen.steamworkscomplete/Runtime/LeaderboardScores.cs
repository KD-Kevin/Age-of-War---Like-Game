#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;
using System;
using System.Collections.Generic;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct LeaderboardScores
    {
        public bool bIOFailure;
        public bool playerIncluded;
        public List<LeaderboardEntry> scoreData;
    }
}
#endif
