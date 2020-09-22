using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace ELS
{
    public static class Extentions
    {
        internal static int lastTick = Game.GameTime;
        public static bool IsEls(this Vehicle vehicle)
        {
            return Vehicle.Exists(vehicle) && vehicle.IsNetworked() && configuration.VCF.ELSVehicle.ContainsKey(vehicle.Model);
        }

        public static bool ToBoolean(this object obj)
        {
            return obj.ToString() == "1" ? true : false;
        }

        public static int ToInt(this bool obj)
        {
            return obj ? 1 : 0;
        }

        public static bool IsSittingInDriverOrPassengerSeat(this Ped ped, Vehicle vehicle)
        {
            return ELS.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == ped || ELS.CurrentVehicle.GetPedOnSeat(VehicleSeat.Passenger) == ped;
        }
        
        public static bool IsSittingInELSVehicle(this Ped ped)
        {
            return ELS.CurrentVehicle?.IsEls() ?? false; 
        }

        async public static Task<bool> RequestCollision(this Vehicle vehicle)
        {

            CitizenFX.Core.Native.Function.Call(Hash.REQUEST_COLLISION_AT_COORD, vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z);
            while (Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, vehicle))
            {
                await BaseScript.Delay(0);
            }
#if DEBUG
            Utils.DebugWriteLine("collision loaded");
#endif
            return true;
        }
        //public static void CleanUp(this PoolObject poolObject)
        //{
        //    if (!poolObject.Exists()) poolObject.CleanUp();
        //}

        public static int GetNetworkId(this Entity entity)
        {
            if (entity.IsNetworked())
            {
                var netId = entity.NetworkId;
                if (API.NetworkDoesNetworkIdExist(netId) && entity.Handle != netId)
                {
                    return netId;
                } else
                {
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// Checks if entity exists and is networked, and only tries to fetch NetworkId if entity is networked.
        /// Returns 0 if NetworkId is not found!
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static int TryGetNetworkId(this Entity entity)
        {
            if (Entity.Exists(entity) && API.NetworkGetEntityIsNetworked(entity.Handle))
            {
                var netId = entity.NetworkId;
                return netId;
            }
            else
                return 0;
        }

        /// <summary>
        /// Checks if entity exists and is networked, and only tries to fetch NetworkId if entity is networked.
        /// Returns 0 if NetworkId is not found!
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public static int TryGetNetworkId(this Vehicle vehicle)
        {
            if (Vehicle.Exists(vehicle) && API.NetworkGetEntityIsNetworked(vehicle.Handle))
            {
                var netId = vehicle.NetworkId;
                return netId;
            }
            else
                return 0;
        }

        public static void RegisterAsNetworked(this Entity entity)
        {
            Function.Call((Hash)0x06FAACD625D80CAA, entity.Handle);
        }

        public static void SetExistOnAllMachines(this Entity entity, bool b)
        {
            Function.Call(Hash.SET_NETWORK_ID_EXISTS_ON_ALL_MACHINES, entity.NetworkId, b);
        }

        public static bool IsNetworked(this Entity entity)
        {
            return API.NetworkGetEntityIsNetworked(entity.Handle);
        }
    }
}
