using ELS.FullSync;
using Shared;
using System.Collections.Generic;

namespace ELS.Siren
{
    internal class Tones : IFullSyncComponent
    {
        internal Tone horn;
        internal Tone tone1;
        internal Tone tone2;
        internal Tone tone3;
        internal Tone tone4;

        public Dictionary<string, object> GetData()
        {
            var dic =
                new Dictionary<string, object>
                {
                    {DataNames.Horn, horn.State},
                    {DataNames.tone1, tone1.State},
                    {DataNames.tone2, tone2.State},
                    {DataNames.tone3, tone3.State},
                    {DataNames.tone4, tone4.State}
                };

            return dic;
        }

        public void SetData(IDictionary<string, object> data)
        {
            if (data.TryGetValue(DataNames.Horn, out var _horn))
            {
                horn.SetState((bool)_horn);
            }
            if (data.TryGetValue(DataNames.tone1, out var tone1State))
            {
                tone1.SetState((bool)tone1State);
            }
            if (data.TryGetValue(DataNames.tone2, out var tone2State))
            {
                tone2.SetState((bool)tone2State);
            }
            if (data.TryGetValue(DataNames.tone3, out var tone3State))
            {
                tone3.SetState((bool)tone3State);
            }
            if (data.TryGetValue(DataNames.tone4, out var tone4State))
            {
                tone4.SetState((bool)tone4State);
            }
        }

        public void RunTick()
        {
            tone1.RunTick();
            tone2.RunTick();
            tone3.RunTick();
            tone4.RunTick();
        }
    }
}