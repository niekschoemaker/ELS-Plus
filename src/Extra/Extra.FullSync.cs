using ELS.FullSync;
using Shared;
using System.Collections.Generic;

namespace ELS.Extra
{
    internal partial class Extra : IFullSyncComponent
    {

        public Dictionary<string, object> GetData()
        {
            
            Dictionary<string, object> dic = new Dictionary<string, object>();
           
            dic.Add(DataNames.PatternRunning, IsPatternRunning);
            dic.Add(DataNames.TurnedOn, TurnedOn);
            return dic;
        }

        public void SetData(IDictionary<string, object> data)
        {
            IsPatternRunning = (bool)data[DataNames.PatternRunning];
            TurnedOn = (bool)data[DataNames.TurnedOn];
        }

    }
}
