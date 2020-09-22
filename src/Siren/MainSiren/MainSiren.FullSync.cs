using Shared;
using System.Collections.Generic;

namespace ELS.Siren
{
    partial class Siren
    {
        internal partial class MainSiren
        {


            public Dictionary<string, object> GetData()
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add(DataNames.Interrupted, interupted);
                dic.Add(DataNames.CurrentTone, currentTone.ToString());
                dic.Add(DataNames._enable, _enable);
                return dic;
            }

            public void SetData(IDictionary<string, object> data)
            {
                currentTone = int.Parse(data[DataNames.CurrentTone].ToString());
                interupted = (bool)data[DataNames.Interrupted];
                _enable = (bool)data[DataNames._enable];
            }
        }
    }
}