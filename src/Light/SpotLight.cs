/*
    ELS FiveM - A ELS implementation for FiveM
    Copyright (C) 2017  E.J. Bevenour

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using CitizenFX.Core;
using CitizenFX.Core.Native;
using ELS.configuration;
using ELS.FullSync;
using Shared;
using System;
using System.Collections.Generic;

namespace ELS.Light
{
    class SpotLight : IFullSyncComponent
    {
        private float anglehorizontal = 90f;
        private float angleVertical = 0f;

        private Vector3 dirVector;

        private ILight lights;
        private bool _on;
        internal bool TurnedOn
        {
            get { return _on; }
            set
            {
                _on = value;
                if (!value && Global.ResetTakedownSpotlight)
                {
                    SpotLightReset();
                }
            }
        }
        public SpotLight(ILight light)
        {
            lights = light;
           
        }

        internal void SpotLightReset()
        {
            anglehorizontal = 90f;
            angleVertical = 0f;
        }

        public Dictionary<string, object> GetData()
        {

            Dictionary<string, object> dic = new Dictionary<string, object>();

            dic.Add(DataNames.Horizontal, anglehorizontal);
            dic.Add(DataNames.Vertical, angleVertical);
            dic.Add(DataNames.TurnedOn, TurnedOn);
            return dic;
        }

        public void SetData(IDictionary<string, object> data)
        {
            anglehorizontal = float.Parse(data[DataNames.Horizontal].ToString());
            angleVertical = float.Parse(data[DataNames.Vertical].ToString());
            TurnedOn = (bool)data[DataNames.TurnedOn];
        }

        public void RunTick()
        {
            var vehicle = lights.ElsVehicle.Vehicle;
            if (vehicle == null)
            {
                return;
            }
            if (Game.IsControlPressed(0, Control.PhoneLeft) && Game.PlayerPed.IsSittingInELSVehicle() && Game.PlayerPed.CurrentVehicle.NetworkId == lights.ElsVehicle.NetworkId)
            {
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MoveSpotlightLeft, lights.ElsVehicle, true);
                anglehorizontal++;
                
            }
            if (Game.IsControlPressed(0, Control.PhoneRight) && Game.PlayerPed.IsSittingInELSVehicle() && Game.PlayerPed.CurrentVehicle.NetworkId == lights.ElsVehicle.NetworkId) 
            {
                
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MoveSpotlightRight, lights.ElsVehicle, true);
                anglehorizontal--;
            }
            if (Game.IsControlPressed(0, Control.PhoneUp) && Game.PlayerPed.IsSittingInELSVehicle() && Game.PlayerPed.CurrentVehicle.NetworkId == lights.ElsVehicle.NetworkId)
            {
                
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MoveSpotlightUp, lights.ElsVehicle, true);
                angleVertical++;
            }
            if (Game.IsControlPressed(0, Control.PhoneDown) && Game.PlayerPed.IsSittingInELSVehicle() && Game.PlayerPed.CurrentVehicle.NetworkId == lights.ElsVehicle.NetworkId)   
            {
                
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MoveSpotlightDown, lights.ElsVehicle, true);
                angleVertical--;
            }

            //var spotoffset = Game.Player.Character.CurrentVehicle.GetOffsetPosition(new Vector3(-0.9f, 1.15f, 0.5f));
            
            var off = vehicle.GetPositionOffset(vehicle.Bones[$"window_lf"].Position);

            var spotoffset = vehicle.GetOffsetPosition(off+new Vector3(-.25f,1f,0.1f));
            
            //Vector3 myPos = Game.PlayerPed.CurrentVehicle.Bones[$"extra_{_id}"].Position;
            float hx = (float)(spotoffset.X + 5 * Math.Cos(((double)anglehorizontal + vehicle.Rotation.Z) * Math.PI / 180.0));
            float hy = (float)(spotoffset.Y + 5 * Math.Sin(((double)anglehorizontal + vehicle.Rotation.Z) * Math.PI / 180.0));
            float vz = (float)(spotoffset.Z + 5 * Math.Sin(angleVertical * Math.PI / 180.0));

            Vector3 destinationCoords = (new Vector3(hx, hy, vz));
            
            dirVector = destinationCoords - spotoffset;
            dirVector.Normalize();
            //Function.Call(Hash.DRAW_SPOT_LIGHT, spotoffset.X, spotoffset.Y, spotoffset.Z, dirVector.X, dirVector.Y, dirVector.Z, 255, 255, 255, 100.0f, 1f, 0.0f, 13.0f, 1f,100f);
            API.DrawSpotLightWithShadow(spotoffset.X,spotoffset.Y,spotoffset.Z, dirVector.X,dirVector.Y,dirVector.Z, 255,255,255,Global.TkdnRng,Global.TkdnInt,1f,18f,1f,0);
        }

    }
}
