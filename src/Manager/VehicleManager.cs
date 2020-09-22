﻿
using CitizenFX.Core;
using CitizenFX.Core.Native;
using ELS.Light;
using ELSShared;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ELS.RemoteEventManager;

namespace ELS.Manager
{
    class VehicleManager
    {
        internal static VehicleList vehicleList;
        internal static bool WasPaused = false;
        static bool notified = false;
        public VehicleManager()
        {
            vehicleList = new VehicleList();
        }

        internal static void MakeNetworked(Vehicle veh)
        {
            //////////
            ///
            ///  THANKS to Antivirus-chan in the FiveM community for supplying this code
            ///
            //////////
            var attempts = 0;
            var netId = API.VehToNet(veh.Handle);
            do
            {
                //BaseScript.Delay(500);
                API.NetworkRegisterEntityAsNetworked(veh.Handle);
                API.SetEntityAsMissionEntity(veh.Handle, false, false);
                if (veh.IsNetworked())
                {
                    API.SetNetworkIdCanMigrate(netId, true);
                    API.SetNetworkIdExistsOnAllMachines(netId, true);
                }
                API.NetworkRequestControlOfEntity(veh.Handle);
                attempts++;
            }
            while (!veh.IsNetworked() && attempts < 20);
            if (attempts == 20 && !notified)
            {
                CitizenFX.Core.Debug.WriteLine("Failed to register entity on net");
                notified = true;
            }
            else if (!notified)
            {
                Debug.WriteLine($"Registered {veh.Handle} on net as {veh.NetworkId}");
                BaseScript.TriggerEvent("ELS:VehicleEntered");
                notified = true;
            }
        }
        internal void RunTick()
        {
            try
            {
                // If in ELS vehicle
                if (ELS.ped.IsSittingInELSVehicle() && ELS.ped.IsSittingInDriverOrPassengerSeat(ELS.CurrentVehicle))
                {
                    API.SetVehicleDeformationFixed(ELS.CurrentVehicle.Handle);
                    var netId = ELS.CurrentVehicle.NetworkId;
                    if (API.NetworkDoesNetworkIdExist(netId))
                    {
                        if (vehicleList.ContainsKey(netId))
                        {
                            ELSVehicle _currentVehicle = vehicleList[netId];
                            _currentVehicle?.RunControlTick();
                            ELS.CurrentVehicle.SetExistOnAllMachines(true);
                        }
                        else
                        {
                            if (!vehicleList.VehRegAttempts.ContainsKey(netId) || Game.GameTime - vehicleList.VehRegAttempts[netId].Item2 >= 15000 && vehicleList.VehRegAttempts[netId].Item1 < 5)
                            {
                                if (ELS.CurrentVehicle.IsNetworked() && vehicleList.MakeSureItExists(netId, vehicle: out ELSVehicle _currentVehicle))
                                {
                                    _currentVehicle?.RunControlTick();
                                    ELS.CurrentVehicle.SetExistOnAllMachines(true);
                                }
                            }
                        }
                    }
                }
                if (ELS.CurrentVehicle != null)
                {
                    var isPaused = Game.IsPaused;
                    if (isPaused)
                    {
                        if (!Indicator.IsHazardLightActive)
                        {
                            WasPaused = true;
                            Indicator.SetHazards(true);
                        }
                    }
                    else if (WasPaused && Indicator.IsHazardLightActive)
                    {
                        WasPaused = false;
                        Indicator.SetHazards(false);
                    }
                }
                vehicleList.RunTick();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"VehicleManager Error: {e.Message} \n Stacktrace: {e.StackTrace}");
            }
        }

        public void SetVehicleLightData(IDictionary<string, object> dataDic, int NetworkId, int PlayerId)
        {
            if (Game.Player.ServerId == PlayerId)
            {
                return;
            }

            if (vehicleList.TryGetValue(NetworkId, out var vehicle))
            {
                vehicle.SetLightData(dataDic);
            }
            else
            {
                Utils.ReleaseWriteLine($"Vehicle with key {NetworkId} not found");
                BaseScript.TriggerServerEvent(EventNames.FullSyncRequestOne, NetworkId);
            }
        }

        public void SetVehicleSirenData(IDictionary<string, object> dataDic, int NetworkId, int PlayerId)
        {
            if (Game.Player.ServerId == PlayerId)
            {
                return;
            }
            
            if (vehicleList.TryGetValue(NetworkId, out var vehicle))
            {
                vehicle.SetSirenData(dataDic);
            }
            else
            {
                Utils.ReleaseWriteLine($"Vehicle with key {NetworkId} not found");
                BaseScript.TriggerServerEvent(EventNames.FullSyncRequestOne, NetworkId);
            }
        }

        /// <summary>
        /// Proxies the sync data to a certain vehicle
        /// </summary>
        /// <param name="dataDic">data</param>
        public void SetVehicleSyncData(IDictionary<string, object> dataDic, int networkId, int PlayerId)
        {
            if (Game.Player.ServerId == PlayerId)
            {
                Utils.DebugWriteLine("Data discarded because PlayerId same as local player ID");
                return;
            }

            Utils.DebugWriteLine($"{PlayerId} has sent us data parsing");

            var hasSirenOrLightData = dataDic.ContainsKey(DataNames.Siren) || dataDic.ContainsKey(DataNames.Light);
            if (vehicleList.ContainsKey(networkId) && hasSirenOrLightData)
            {
                vehicleList[networkId].SetData(dataDic);
                return;
            }
            if (hasSirenOrLightData && (!vehicleList.VehRegAttempts.ContainsKey(networkId) || (Game.GameTime - vehicleList.VehRegAttempts[networkId].Item2 >= 15000 && vehicleList.VehRegAttempts[networkId].Item1 < 5)))
            {
                if (!vehicleList.MakeSureItExists(networkId, dataDic, out ELSVehicle veh1, PlayerId))
                {
                    Utils.ReleaseWriteLine($"Failed to register other clients vehicle with id {networkId}");
                    return;
                }
            }

            if (dataDic.ContainsKey(DataNames.IndicatorState) && !hasSirenOrLightData && API.NetworkDoesNetworkIdExist(networkId))
            {
                if(API.NetworkDoesNetworkIdExist(networkId))
                {
                    Vehicle veh = (Vehicle)Entity.FromHandle(API.NetworkGetEntityFromNetworkId(networkId));
                    if (veh != null)
                    {
                        Indicator.ToggleInicatorState(veh, Indicator.IndStateLib[dataDic[DataNames.IndicatorState].ToString()]);
                    }
                }
            }
        }

        internal static void SyncUI(int netId)
        {
            if (netId == 0)
            {
#if DEBUG
                Utils.DebugWriteLine("Vehicle net ID is empty");
#endif
                return;
            }
            vehicleList[netId].SyncUi();
        }

        internal static void SyncRequestReply(Commands command, int NetworkId)
        {
            if (NetworkId == 0 || !API.NetworkDoesNetworkIdExist(NetworkId))
            {
                return;
            }
            switch (command)
            {
                case Commands.ToggleInd:
                    Dictionary<string, object> dict = new Dictionary<string, object>
                    {
                        {DataNames.IndicatorState, Indicator.CurrentIndicatorState((Vehicle)Vehicle.FromHandle(API.NetworkGetEntityFromNetworkId(NetworkId))).ToString() }
                    };
                    FullSync.FullSyncManager.SendDataBroadcast(dict, NetworkId);
                    break;
                default:
                    var data = vehicleList[NetworkId].GetData();
                    FullSync.FullSyncManager.SendDataBroadcast(data, NetworkId);
                    break;

            }
        }

        internal void SyncAllVehiclesOnFirstSpawn(System.Dynamic.ExpandoObject data)
        {
            //dynamic k = data;
            var y = data.ToArray();
            foreach (var struct1 in y)
            {
                int netID = int.Parse(struct1.Key);
                var vehData = (IDictionary<string, object>)struct1.Value;
                vehicleList.MakeSureItExists(netID, vehData, out ELSVehicle _);
            }
        }

        internal void CleanUP()
        {
            vehicleList.CleanUP();
        }
        ~VehicleManager()
        {
            CleanUP();
        }
    }
}
