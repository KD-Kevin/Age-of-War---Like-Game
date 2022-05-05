﻿#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="PersonaStateChange_t"/> data.
    /// </summary>
    [Serializable]
    public class PersonaStateChangeEvent : UnityEvent<PersonaStateChange_t>
    { }
}
#endif