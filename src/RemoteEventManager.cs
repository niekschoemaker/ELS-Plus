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
using System.Collections.Generic;

namespace ELS
{
    class RemoteEventManager
    {
        internal RemoteEventManager()
        {
             
        }

        internal enum Commands
        {
            MainSiren,
            AirHorn,
            ManualTone1,
            ManualTone2,
            ManualTone3,
            ManualTone4,
            ManualSound,
            DualSiren,
            PanicAlarm,
            ToggleWrnL,
            ToggleSecL,
            ToggleCrsL,
            ToggleTDL,
            ToggleSCL,
            ChangePrmPatt,
            ChangeSecPatt,
            ChangeWrnPatt,
            ToggleLstg,
            ChgPattPrmL,
            ChgPattWrnL,
            MoveSpotlightUp,
            MoveSpotlightDown,
            MoveSpotlightLeft,
            MoveSpotlightRight,
            MoveLadderUp,
            MoveLadderDown,
            MoveLadderLeft,
            MoveLadderRight,
            ToggleInd,
            ToggleBrd,
            FullSync
        }
        internal enum MessageTypes
        {
            SirenUpdate,
            SirenAdded,
            SirenRemoved,
            LightUpdate
        }
        internal delegate void RemoteMessageRecievedHandler();
        internal static event RemoteMessageRecievedHandler RemoteMessageRecieved;

        internal static void SendEvent(Commands type, Vehicle vehicle, bool state)
        {
            Debug.WriteLine($"sendding data for netID {vehicle.NetworkId} : {state}");
            Manager.VehicleManager.SyncRequestReply(type, vehicle.NetworkId);
        }

        internal static void SendEvent(Commands type, ELSVehicle vehicle, bool state)
        {
            Debug.WriteLine($"sendding data for netID {vehicle.NetworkId} of type {type} : {state}");
            Manager.VehicleManager.SyncRequestReply(type, vehicle.NetworkId);
        }

        internal static void SendEvent(ELSVehicle vehicle, Dictionary<string, object> data)
        {
            Debug.WriteLine($"sendding data for netID {vehicle.NetworkId}");
            FullSync.FullSyncManager.SendDataBroadcast(data, vehicle.NetworkId);
        }

        internal static void SendLightEvent(ELSVehicle vehicle, string key, object value)
        {
            Debug.WriteLine($"sendding LightSync data for netID {vehicle.NetworkId}; {key} : {value}");
            var data = new Dictionary<string, object>
            {
                { key, value }
            };
            SendLightEvent(vehicle, data);
        }

        internal static void SendLightEvent(ELSVehicle vehicle, Dictionary<string, object> data)
        {
            Debug.WriteLine($"sendding LightSync data for netID {vehicle.NetworkId}");
            FullSync.FullSyncManager.SendLightBroadcast(data, vehicle.NetworkId);
        }

        internal static void SendSirenEvent(ELSVehicle vehicle, string key, object value)
        {
            Debug.WriteLine($"sendding SirenSync data for netID {vehicle.NetworkId}; {key} : {value}");

            var data = new Dictionary<string, object>
            {
                { key, value }
            };
            SendSirenEvent(vehicle, data);
        }

        internal static void SendSirenEvent(ELSVehicle vehicle, Dictionary<string, object> data)
        {
            Debug.WriteLine($"sendding SirenSync data for netID {vehicle.NetworkId}");
            FullSync.FullSyncManager.SendSirenBroadcast(data, vehicle.NetworkId);
        }
    }
}
