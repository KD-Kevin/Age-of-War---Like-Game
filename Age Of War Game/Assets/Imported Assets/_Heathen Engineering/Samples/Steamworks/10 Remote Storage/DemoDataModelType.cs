#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using System;

namespace HeathenEngineering.DEMO
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    [Serializable]
    public struct DemoDataModelType
    {
        public string stringData;
        public int intData;
        public bool boolData;
    }
}
#endif
