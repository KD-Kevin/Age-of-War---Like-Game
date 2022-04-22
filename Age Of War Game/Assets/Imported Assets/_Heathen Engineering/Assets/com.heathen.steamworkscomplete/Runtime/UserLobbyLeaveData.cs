#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public struct UserLobbyLeaveData
    {
        public UserData user;
        public Steamworks.EChatMemberStateChange state;
    }
}
#endif