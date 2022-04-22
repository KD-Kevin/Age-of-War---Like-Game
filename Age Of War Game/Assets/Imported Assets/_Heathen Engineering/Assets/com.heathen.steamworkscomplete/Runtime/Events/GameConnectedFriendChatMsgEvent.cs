#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public class GameConnectedFriendChatMsgEvent : UnityEvent<UserData, string, EChatEntryType> { }
}
#endif