using ELSShared;
using System.Collections;
using System.Collections.Generic;
using CitizenFX.Core;

namespace ELS.FullSync
{
    public class FullSync
    {
        protected FullSync()
        {

        }
    }

    internal class FullSyncManager
    {
        internal FullSyncManager()
        {
            
        }

        internal static void SendDataBroadcast(IDictionary dic, int NetworkId)
        {
            BaseScript.TriggerServerEvent(EventNames.FullSyncBroadcast, dic, NetworkId);
        }

        internal static void SendLightBroadcast(IDictionary dic, int NetworkId)
        {

            BaseScript.TriggerServerEvent(EventNames.LightSyncBroadcast, dic, NetworkId);
        }

        internal static void SendSirenBroadcast(IDictionary dic, int NetworkId)
        {
            BaseScript.TriggerServerEvent(EventNames.SirenSyncBroadcast, dic, NetworkId);
        }
    }

    internal interface IFullSyncComponent
    {
        void SetData(IDictionary<string, object> data);
        Dictionary<string, object> GetData();
    }
}