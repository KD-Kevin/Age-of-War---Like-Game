﻿#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="UserAchievementStored_t"/> data.
    /// </summary>
    [Serializable]
    public class UserAchievementStoredEvent : UnityEvent<UserAchievementStored_t>
    { }
}
#endif
