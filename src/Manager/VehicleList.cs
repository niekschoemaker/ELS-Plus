﻿using CitizenFX.Core.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ELS.Manager
{
    class VehicleList : List<ELSVehicle>
    {
        public new void Add(ELSVehicle veh)
        { 
            base.Add(veh);
        }
        public void Add(int NetworkID)
        {
            var veh = new ELSVehicle(API.NetToVeh(NetworkID));
            Add(veh);
        }
        public bool IsReadOnly => throw new NotImplementedException();
        public void RunTick()
        {

        }
        public void RunExternalTick()
        {
            foreach(var t in this)
            {
                t.RunExternalTick();
            }
        }
        public bool MakeSureItExists(int NetworkID, [Optional]out ELSVehicle vehicle)
        {
            if (NetworkID == 0)
            {
                CitizenFX.Core.Debug.WriteLine("ERROR Try to add vehicle\nNetwordID equals 0\n");
                vehicle = null;
                return false;
            }

            else if (!Exists(poolObject => (poolObject).GetNetworkId() == NetworkID))
            {
                try
                {
                    var veh = new ELSVehicle(API.NetToVeh(NetworkID));
                    Add(veh);
                    vehicle = veh;
                    return true;
                }
                catch (Exception ex)
                {
                    vehicle = null;
                    return false;
                    throw ex;
                }

            }
            else
            {
                vehicle = Find(poolObject => ((ELSVehicle)poolObject).GetNetworkId() == NetworkID);
                return true;
            }
        }
        public bool MakeSureItExists(int NetworkID, IDictionary<string, object> data, [Optional]out ELSVehicle vehicle)
        {
            if (NetworkID == 0)
            {
                CitizenFX.Core.Debug.WriteLine("ERROR NetwordID equals 0\n");
                vehicle = null;
                return false;
            }

            else if (!Exists(poolObject => (poolObject).GetNetworkId() == NetworkID))
            {
                try
                {
                    var veh = new ELSVehicle(API.NetToVeh(NetworkID),data);
                    Add(veh);
                    vehicle = veh;
                    return true;
                }
                catch (Exception ex)
                {
                    vehicle = null;
                    return false;
                    throw ex;
                }

            }
            else
            {
                vehicle = Find(poolObject => ((ELSVehicle)poolObject).GetNetworkId() == NetworkID) as ELSVehicle;
                return true;
            }
        }
        public void CleanUP()
        {
            ForEach((veh) => { veh.CleanUP();});
        }
        ~VehicleList()
        {
            CleanUP();
            Clear();
        }
    }
}
