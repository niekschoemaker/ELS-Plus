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
                if (_on == value) return;
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
        }

        public void RunTick()
        {
            var vehicle = iLight.ElsVehicle.Vehicle;
            if (vehicle == null)
            {
                return;
            }
            var loff = vehicle.GetPositionOffset(vehicle.Bones[$"window_lf"].Position);
            var lspotoffset = vehicle.GetOffsetPosition(loff + new Vector3(-0.5f, -0.5f, 0.5f));

            float lhx = (float)(lspotoffset.X + 5 * Math.Cos(((double)-180 + vehicle.Rotation.Z) * Math.PI / 180.0));
            float lhy = (float)(lspotoffset.Y + 5 * Math.Sin(((double)-180 + vehicle.Rotation.Z) * Math.PI / 180.0));
            float lvz = (float)(lspotoffset.Z + 5 * Math.Sin(-180 * Math.PI / 180.0));

            Vector3 ldestinationCoords = (new Vector3(lhx, lhy, lvz));

            ldirVector = ldestinationCoords - lspotoffset;
            ldirVector.Normalize();

            var roff = vehicle.GetPositionOffset(vehicle.Bones[$"window_rf"].Position);
            var rspotoffset = vehicle.GetOffsetPosition(roff + new Vector3(0.5f, -0.5f, 0.5f));

            float rhx = (float)(rspotoffset.X + 5 * - Math.Cos(((double)180 + vehicle.Rotation.Z) * Math.PI / 180.0));
            float rhy = (float)(rspotoffset.Y + 5 * - Math.Sin(((double)180 + vehicle.Rotation.Z) * Math.PI / 180.0));
            float rvz = (float)(rspotoffset.Z + 5 * - Math.Sin(180 * Math.PI / 180.0));

            Vector3 rdestinationCoords = (new Vector3(rhx, rhy, rvz));

            rdirVector = rdestinationCoords - rspotoffset;
            rdirVector.Normalize();

            API.DrawSpotLightWithShadow(lspotoffset.X, lspotoffset.Y, lspotoffset.Z, ldirVector.X, ldirVector.Y, ldirVector.Z, 255, 255, 255, Global.TkdnRng, Global.TkdnInt, 0f, 100f, 1f, 1);
            API.DrawSpotLightWithShadow(rspotoffset.X, rspotoffset.Y, rspotoffset.Z, rdirVector.X, rdirVector.Y, rdirVector.Z, 255, 255, 255, Global.TkdnRng, Global.TkdnInt, 0f, 100f, 1f, 2);
        }
    }
}
