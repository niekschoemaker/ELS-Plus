﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ELS.Manager
{
    class VehicleList : Dictionary<int,ELSVehicle>
    {
        internal Dictionary<int,Tuple<int,int>> VehRegAttempts;
        //public new void Add(ELSVehicle veh)
        //{
        //    base.Add(veh);
        //}

        internal VehicleList()
        {
            VehRegAttempts = new Dictionary<int,Tuple<int,int>>();
        }
        public void Add(int NetworkID)
        {
            var veh = new ELSVehicle(API.NetToVeh(NetworkID));
            Add(NetworkID,veh);
        }
        public bool IsReadOnly => throw new NotImplementedException();

        public void RunTick(bool inVehicle = false)
        {
            for(int i = 0; i < Count; i++)
            {
                this.ElementAt(i).Value.RunTick();
            }
        }

        public void RunExternalTick([Optional] ELSVehicle vehicle)
        {
            try
            {
                for (int i = 0; i < Count; i++)
                //foreach (var t in Values)
                {
                    if (vehicle == null || this.ElementAt(i).Value.Handle != vehicle.Handle)
                    {
                        //t.RunExternalTick();
                        this.ElementAt(i).Value.RunTick();
                    } 
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Utils.DebugWriteLine($"VehicleList Error: {e.Message}");
#endif
            }
        }

        public bool MakeSureItExists(int NetworkID, [Optional]out ELSVehicle vehicle)
        {
            if (NetworkID == 0)
            {
                CitizenFX.Core.Debug.WriteLine("ERROR Try to add vehicle\nNetwordID equals 0");
                vehicle = null;
                return false;
            }

            else if (!ContainsKey(NetworkID))
            {
                if (VehRegAttempts.ContainsKey(NetworkID))
                {
                    VehRegAttempts[NetworkID] = new Tuple<int, int>(VehRegAttempts[NetworkID].Item1 + 1, Game.GameTime);
                }
                else
                {
                    VehRegAttempts.Add(NetworkID, new Tuple<int, int>(1, Game.GameTime));
                }
                try
                {
                    ELSVehicle veh = null;
                    int handle = API.NetToVeh(NetworkID);
                    if (handle == 0)
                    {
                        veh = new ELSVehicle(Game.PlayerPed.CurrentVehicle.Handle);
                    }
                    else
                    {
                        veh = new ELSVehicle(handle);
                    }
                    Add(NetworkID,veh);
                    //CurrentlyRegisteringVehicle.Remove(NetworkID);
                    vehicle = veh;
                    return true;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Utils.DebugWriteLine($"Exsits Error: {ex.Message} due to {ex.InnerException}");
#endif
                    //CurrentlyRegisteringVehicle.Remove(NetworkID);
                    vehicle = null;
                    return false;
                    throw ex;
                }

            }
            else
            {
                vehicle = this[NetworkID];//Find(poolObject => ((ELSVehicle)poolObject).GetNetworkId() == NetworkID);
                return true;
            }
        }

        public bool MakeSureItExists(int NetworkID, IDictionary<string, object> data,  [Optional]out ELSVehicle vehicle, int PlayerId = -1)
        {
            if (NetworkID == 0)
            {
                CitizenFX.Core.Debug.WriteLine("ERROR NetwordID equals 0\n");
                vehicle = null;
                return false;
            }

            else if (!ContainsKey(NetworkID))
            {
                if (VehRegAttempts.ContainsKey(NetworkID))
                {
                    VehRegAttempts[NetworkID] = new Tuple<int, int>(VehRegAttempts[NetworkID].Item1 + 1, Game.GameTime);
                }
                else
                {
                    VehRegAttempts.Add(NetworkID, new Tuple<int, int>(1, Game.GameTime));
                }
                try
                {
                    ELSVehicle veh = null;
                    int handle = API.NetworkGetEntityFromNetworkId(NetworkID);
                    if (handle == 0 && PlayerId != -1)
                    {
                        Player player = new Player(API.GetPlayerFromServerId(PlayerId));
#if DEBUG
                        Utils.DebugWriteLine($"Registering vehicle with netid of {NetworkID} to list from {player.Name}");
#endif
                        if (!player.Character.IsSittingInELSVehicle())
                        {
#if DEBUG
                            Utils.DebugWriteLine($"{player.Name} is not in an Els vehicle");
#endif
                            vehicle = null;
                            return false;
                        }
                        veh = new ELSVehicle(player.Character.CurrentVehicle.Handle, data);
                    }
                    else
                    {
#if DEBUG
                        Utils.DebugWriteLine($"Registering vehicle {NetworkID} to list from netid to veh");
#endif
                        veh = new ELSVehicle(handle,data);
                    }
                    if (veh != null)
                    {
                        //CurrentlyRegisteringVehicle.Remove(NetworkID);
                        Add(NetworkID, veh);
#if DEBUG
                        Utils.DebugWriteLine($"Added {NetworkID} to vehicle list");
#endif
                        vehicle = veh;
                        return true;
                    } 
                    else
                    {
                        //CurrentlyRegisteringVehicle.Remove(NetworkID);
#if DEBUG
                        Utils.DebugWriteLine("Failed to add vehicle to list please try again");
#endif
                        vehicle = null;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    //CurrentlyRegisteringVehicle.Remove(NetworkID);
#if DEBUG
                    Utils.DebugWriteLine($"Exsits Error With Data: {ex.Message}");
#endif
                    vehicle = null;
                    return false;
                    throw ex;
                }

            }
            else
            {
#if DEBUG
                Utils.DebugWriteLine($"Returning vehicle {NetworkID} from list");
#endif
                vehicle = this[NetworkID];
                return true;
            }
        }

        
        public void CleanUP()
        {
            for(int i = 0; i < Count; i++)
            {
                this.ElementAt(i).Value.CleanUP();
            }
        }
        ~VehicleList()
        {
            CleanUP();
            Clear();
        }
    }
}
