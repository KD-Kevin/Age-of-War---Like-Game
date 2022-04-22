#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    public struct DlcData
    {
        public AppId_t AppId { get; private set; }
        public bool Available { get; private set; }
        public string Name { get; private set; }

        public DlcData(AppId_t id, bool available, string name)
        {
            AppId = id;
            Available = available;
            Name = name;
        }
    }
}
#endif