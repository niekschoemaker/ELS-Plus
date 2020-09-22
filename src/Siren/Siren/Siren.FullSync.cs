using ELS.FullSync;
using Shared;
using System.Collections.Generic;

namespace ELS.Siren
{
    partial class Siren : IManagerEntry, IFullSyncComponent
    {
        public void SetData(IDictionary<string, object> data)
        {
            if (data.TryGetValue(DataNames._mainSiren, out var mainSiren))
            {
                _mainSiren.SetData((IDictionary<string, object>)mainSiren);
            }
            if (data.TryGetValue(DataNames._tones, out var tones)) {
                _tones.SetData((IDictionary<string,object>)tones);
            }
            if(data.TryGetValue(DataNames.dual_siren, out var dualSiren))
            {
                dual_siren = (bool)dualSiren;
            }
        }

        public Dictionary<string, object> GetData()
        {
            var dic = new Dictionary<string, object>
            {
                { DataNames._mainSiren, _mainSiren.GetData() },
                { DataNames._tones, _tones.GetData() },
                { DataNames.dual_siren, dual_siren }
            };
            return dic;
        }
    }
}