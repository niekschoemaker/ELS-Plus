using CitizenFX.Core;
using CitizenFX.Core.Native;
using ELS.configuration;
using Shared;
using System;
using System.Collections.Generic;

namespace ELS.Light
{
    internal class Scene
    {
        readonly ILight iLight;
        private Vector3 ldirVector;
        private Vector3 rdirVector;
        private bool _on;

        internal bool TurnedOn
        {
            get { return _on; }
            set
            {

                _on = value;
                if (!iLight.Vcfroot.MISC.SceneLights.IlluminateSidesOnly)
                {
                    iLight.SpotLight.TurnedOn = TurnedOn;
                }
            }
        }
        internal Scene(ILight light)
        {
            iLight = light;
            TurnedOn = false;
        }

        public Dictionary<string, object> GetData()
        {

            Dictionary<string, object> dic = new Dictionary<string, object>();


            dic.Add(DataNames.TurnedOn, TurnedOn);
            return dic;
        }

        public void SetData(IDictionary<string, object> data)
        {

            TurnedOn = (bool)data[DataNames.TurnedOn];
#if DEBUG
            Utils.DebugWriteLine($"Got scene data for {iLight._vehicle.GetNetworkId()}");
#endif
        }

        public void RunTick()
        {
            var loff = iLight.Vehicle.GetPositionOffset(iLight.Vehicle.Bones[$"window_lf"].Position);
            var lspotoffset = iLight.Vehicle.GetOffsetPosition(loff + new Vector3(-0.5f, -0.5f, 0.5f));

            float lhx = (float)((double)lspotoffset.X + 5 * Math.Cos(((double)-180 + iLight.Vehicle.Rotation.Z) * Math.PI / 180.0));
            float lhy = (float)((double)lspotoffset.Y + 5 * Math.Sin(((double)-180 + iLight.Vehicle.Rotation.Z) * Math.PI / 180.0));
            float lvz = (float)((double)lspotoffset.Z + 5 * Math.Sin((double)-180 * Math.PI / 180.0));

            Vector3 ldestinationCoords = (new Vector3(lhx, lhy, lvz));

            ldirVector = ldestinationCoords - lspotoffset;
            ldirVector.Normalize();

            var roff = iLight.Vehicle.GetPositionOffset(iLight.Vehicle.Bones[$"window_rf"].Position);
            var rspotoffset = iLight.Vehicle.GetOffsetPosition(roff + new Vector3(0.5f, -0.5f, 0.5f));

            float rhx = (float)((double)rspotoffset.X + 5 * - Math.Cos(((double)180 + iLight.Vehicle.Rotation.Z) * Math.PI / 180.0));
            float rhy = (float)((double)rspotoffset.Y + 5 * - Math.Sin(((double)180 + iLight.Vehicle.Rotation.Z) * Math.PI / 180.0));
            float rvz = (float)((double)rspotoffset.Z + 5 * - Math.Sin((double)180 * Math.PI / 180.0));

            Vector3 rdestinationCoords = (new Vector3(rhx, rhy, rvz));

            rdirVector = rdestinationCoords - rspotoffset;
            rdirVector.Normalize();

            API.DrawSpotLightWithShadow(lspotoffset.X, lspotoffset.Y, lspotoffset.Z, ldirVector.X, ldirVector.Y, ldirVector.Z, 255, 255, 255, Global.TkdnRng, Global.TkdnInt, 0f, 100f, 1f, 1);
            API.DrawSpotLightWithShadow(rspotoffset.X, rspotoffset.Y, rspotoffset.Z, rdirVector.X, rdirVector.Y, rdirVector.Z, 255, 255, 255, Global.TkdnRng, Global.TkdnInt, 0f, 100f, 1f, 2);


        }
    }
}
