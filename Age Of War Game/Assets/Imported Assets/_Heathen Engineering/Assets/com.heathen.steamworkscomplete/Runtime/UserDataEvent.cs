#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public class UserDataEvent : UnityEvent<UserData> { }
}
#endif