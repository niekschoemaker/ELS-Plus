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
                if (data.TryGetValue(DataNames.CurrentTone, out var tone))
                {
                    setMainTone(int.Parse(tone.ToString()));
                    currentTone = int.Parse(tone.ToString());
                }
                if (data.TryGetValue(DataNames.Interrupted, out var interrupted))
                {
                    interupted = (bool)interrupted;
                }
                if (data.TryGetValue(DataNames._enable, out var enable))
                {
                    SetEnable((bool)enable);
                }
                
                
            }
        }
    }
}