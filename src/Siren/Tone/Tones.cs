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
            horn.SetState((bool)data[DataNames.Horn]);
            tone1.SetState((bool)data[DataNames.tone1]);
            tone2.SetState((bool)data[DataNames.tone2]);
            tone3.SetState((bool)data[DataNames.tone3]);
            tone4.SetState((bool)data[DataNames.tone4]);
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