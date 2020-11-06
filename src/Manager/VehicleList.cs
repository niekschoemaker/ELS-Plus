using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ELS.Manager
{
    class VehicleList : Dictionary<int,ELSVehicle>
    {
        internal Dictionary<int, object> RawData;
        internal Dictionary<int,Tuple<int,int>> VehRegAttempts = new Dictionary<int, Tuple<int, int>>();

        internal VehicleList()
        {}

        public void Add(int NetworkID)
        {
            var veh = new ELSVehicle(API.NetToVeh(NetworkID), NetworkID);
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
                Utils.DebugWriteLine($"VehicleList Error: {e.Message}");
            }
        }

        public bool MakeSureItExists(int NetworkID, [Optional]out ELSVehicle vehicle)
        {
            if (NetworkID == 0)
            {
                Debug.WriteLine("ERROR Try to add vehicle\nNetwordID equals 0");
                vehicle = null;
                return false;
            }

            else if (!ContainsKey(NetworkID))
            {
                if (VehRegAttempts.ContainsKey(NetworkID))
                {
                    VehRegAttempts[NetworkID] = new Tuple<int, int>(VehRegAttempts[NetworkID].Item1 + 1, ELS.GameTime);
                }
                else
                {
                    VehRegAttempts.Add(NetworkID, new Tuple<int, int>(1, ELS.GameTime));
                }
                try
                {
                    if (API.NetworkDoesNetworkIdExist(NetworkID))
                    {
                        ELSVehicle veh = null;
                        int handle = API.NetToVeh(NetworkID);
                        if (handle == 0)
                        {
                            veh = new ELSVehicle(ELS.CurrentVehicle.Handle, ELS.CurrentVehicle.NetworkId);
                        }
                        else
                        {
                            veh = new ELSVehicle(handle, NetworkID);
                        }
                        Add(NetworkID, veh);
                        vehicle = veh;
                        return true;
                    }
                    else
                    {
                        vehicle = null;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Utils.DebugWriteLine($"Exsits Error: {ex.Message} due to {ex.InnerException} {ex.StackTrace} {ex}");
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
                Debug.WriteLine("ERROR NetwordID equals 0\n");
                vehicle = null;
                return false;
            }

            else if (!ContainsKey(NetworkID))
            {
                if (VehRegAttempts.ContainsKey(NetworkID))
                {
                    VehRegAttempts[NetworkID] = new Tuple<int, int>(VehRegAttempts[NetworkID].Item1 + 1, ELS.GameTime);
                }
                else
                {
                    VehRegAttempts.Add(NetworkID, new Tuple<int, int>(1, ELS.GameTime));
                }
                try
                {
                    ELSVehicle veh = null;
                    // Vehicle is out of scope so create it with just network id
                    if (PlayerId != -1 && !API.NetworkDoesNetworkIdExist(NetworkID))
                    {
                        Utils.DebugWriteLine($"Registering vehicle with netid of {NetworkID} to list from {PlayerId}");
                        veh = new ELSVehicle(0, NetworkID, data);
                        if (veh == null)
                        {
                            vehicle = null;
                            veh = null;
                            return false;
                        }
                    }
                    else
                    {
                        if (API.NetworkDoesNetworkIdExist(NetworkID))
                        {
                            int handle = API.NetworkGetEntityFromNetworkId(NetworkID);
                            veh = new ELSVehicle(handle, NetworkID, data);
                        }
                    }
                    if (veh != null)
                    {
                        //CurrentlyRegisteringVehicle.Remove(NetworkID);
                        Add(NetworkID, veh);
                        Utils.DebugWriteLine($"Added {NetworkID} to vehicle list");
                        vehicle = veh;
                        return true;
                    } 
                    else
                    {
                        //CurrentlyRegisteringVehicle.Remove(NetworkID);
                        Utils.DebugWriteLine("Failed to add vehicle to list please try again");
                        vehicle = null;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    //CurrentlyRegisteringVehicle.Remove(NetworkID);
                    Utils.DebugWriteLine($"Exsits Error With Data: {ex.Message}\n" +
                        $"{ex}");
                    vehicle = null;
                    return false;
                    throw ex;
                }

            }
            else
            {
                Utils.DebugWriteLine($"Returning vehicle {NetworkID} from list");
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
