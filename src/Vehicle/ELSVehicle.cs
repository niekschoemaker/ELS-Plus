using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CitizenFX.Core;
using ELS.configuration;
using CitizenFX.Core.Native;
using ELS.Manager;
using Shared;

namespace ELS
{
    public class ELSVehicle : PoolObject, FullSync.IFullSyncComponent
    {
        public bool Changed { get; set; } = true;
        internal const float maxDistance = 200.0f;
        internal const float deactivateDistance = maxDistance + 10.0f;
        internal const float reactivateDistance = maxDistance - 10.0f;
        internal int lastCheck = 0;
        internal int lastAutoDisable = 0;

#if STATEBAG
        public StateBag State { get; set; }
#endif
        private Siren.Siren _siren;
        private Light.Lights _light;
        private bool Loading = false;
        internal bool IsInitialized = false;
        private Vehicle _vehicle { get => vehicle; set
            {
                lastExists = ELS.GameTime;
                lastExistsValue = Vehicle.Exists(vehicle);
                vehicle = value;
            }
        }
        private Vcfroot _vcf;
        private int _lastTry = -1000;
        public bool IsSirenActive = false;
        internal int cachedNetId;

        public ELSVehicle(int handle, int netId, [Optional] IDictionary<string, object> data) : base(handle)
        {
            cachedNetId = netId;
            if (API.DoesEntityExist(handle))
            {
                Init(handle);
            }
            else if (netId != 0 && API.NetworkDoesNetworkIdExist(netId))
            {
                handle = API.NetToVeh(netId);
                if (handle == 0)
                {
                    handle = API.NetworkGetEntityFromNetworkId(netId);
                }
                Init(handle);
            }

            _light = new Light.Lights(this, _vcf);
            _siren = new Siren.Siren(this, _vcf, (IDictionary<string, object>)data?[DataNames.Siren], _light);

            if (cachedNetId == 0)
            {
                cachedNetId = _vehicle?.NetworkId ?? 0;
            }
        }
        private async void ModelLoaded()
        {
            if (!_vehicle.Model.IsInCdImage)
            {
                throw new Exception($"ELSVehicle.cs:Vehicle {_vehicle.DisplayName} not found in CdImage");
            }
            var start = API.GetGameTimer();
            Loading = true;
            while (_vehicle.DisplayName == "CARNOTFOUND" && API.GetGameTimer() - start < 5000)
            {
                await BaseScript.Delay(0);
            }
            Loading = false;
            if (_vehicle.DisplayName == "CARNOTFOUND")
            {
                throw new Exception("ELSVehicle.cs:Vehicle loading timed out after 10 seconds");
            }
        }

        private void Init(int handle)
        {
            if (!API.DoesEntityExist(handle) || Loading)
            {
                return;
            }
            _vehicle = new Vehicle(handle);
            if (!Vehicle.Exists(_vehicle))
            {
                _vehicle = null;
                IsInitialized = false;
                return;
            }
#if STATEBAG
            State = _vehicle.State;
#endif
            ModelLoaded();
            API.SetVehicleAutoRepairDisabled(_vehicle.Handle, true);
            API.SetVehRadioStation(_vehicle.Handle, "OFF");
            API.SetVehicleRadioEnabled(_vehicle.Handle, false);
            if (_vehicle.DisplayName == "CARNOTFOUND")
            {
                throw new Exception("ELSVehicle.cs:Vehicle not found");
            }
            else if (_vehicle.NetworkId == 0)
            {
                if (IsInitialized)
                {
                    CleanUP();
                    IsInitialized = false;
                }
                _vehicle = null;
                throw new Exception("ELSVehicle.cs:NetworkId is 0");
            }
            else if (VCF.ELSVehicle.ContainsKey(_vehicle.Model))
            {
                _vcf = VCF.ELSVehicle[_vehicle.Model].root;
            }

            try
            {
                API.SetVehicleAutoRepairDisabled(_vehicle.Handle, true);
            }
            catch
            {
                Utils.ReleaseWriteLine("ELSVehicle.cs:Repair Fix is not enabled on this client");
            }

            IsInitialized = true;
        }
        internal void CleanUP(bool tooFarAwayCleanup = false)
        {
            GetVehicle();
            _siren.CleanUP(tooFarAwayCleanup);
            _light.CleanUP(tooFarAwayCleanup);
        }

        Timer getVehicleTimer = new Timer();
        internal Vehicle Vehicle
        {
            get
            {
                if (getVehicleTimer.Expired && !Vehicle.Exists(_vehicle) && API.NetworkDoesNetworkIdExist(cachedNetId))
                {
                    var handle = API.NetworkGetEntityFromNetworkId(cachedNetId);
                    CitizenFX.Core.Debug.WriteLine($"Attempting to init vehicle: net: {cachedNetId} ({handle})");
                    Init(handle);
                    getVehicleTimer.Limit = 2000;
                }
                return _vehicle;
            }
        }

        public Vehicle GetVehicle()
        {
            if (getVehicleTimer.Expired && !Vehicle.Exists(_vehicle) && API.NetworkDoesNetworkIdExist(cachedNetId))
            {
                var handle = API.NetworkGetEntityFromNetworkId(cachedNetId);
                CitizenFX.Core.Debug.WriteLine($"Attempting to init vehicle: net: {cachedNetId} ({handle})");
                Init(handle);
                getVehicleTimer.Limit = 2000;
            }
            return _vehicle;
        }

        internal bool TryGetVehicle(out Vehicle vehicle)
        {
            vehicle = Vehicle;
            return vehicle != null;
        }

        internal void RunControlTick()
        {
            if (!IsInitialized || !Vehicle.Exists(_vehicle))
            {
                if (ELS.GameTime - _lastTry > 1000)
                {
                    _vehicle = null;
                    if (API.NetworkDoesNetworkIdExist(cachedNetId))
                    {
                        Init(API.NetworkGetEntityFromNetworkId(cachedNetId));
                    }
                    else
                    {
                        _lastTry = ELS.GameTime;
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            _siren.ControlTicker(_light);
            _light.ControlTicker();
        }

        DateTime lastRemovalTry = DateTime.Now;
        bool oldSirenState = false;
        internal void RunTick()
        {
            if (!IsInitialized || !Vehicle.Exists(_vehicle))
            {
                if (ELS.GameTime - _lastTry > 1000)
                {
                    _vehicle = null;
                    IsInitialized = false;
                    if (API.NetworkDoesNetworkIdExist(cachedNetId))
                    {
                        _lastTry = ELS.GameTime;
                        Init(API.NetworkGetEntityFromNetworkId(cachedNetId));
                    }
                    else
                    {
                        _lastTry = ELS.GameTime;
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            if (!IsInitialized || !Vehicle.Exists(_vehicle))
            {
                return;
            }

            Function.Call(Hash._SET_DISABLE_VEHICLE_SIREN_SOUND, _vehicle.Handle, true);
            var distance = Vector3.Distance(ELS.Position, _vehicle.Position);
            if (distance > deactivateDistance && IsSirenActive)
            {
                Utils.ReleaseWriteLine($"Stopping siren for netId {cachedNetId}");
                oldSirenState = true;
#if SIREN
                _vehicle.IsSirenActive = false;
#endif
                IsSirenActive = false;
            }
            else if (distance < reactivateDistance && oldSirenState)
            {
                Utils.ReleaseWriteLine($"Starting siren for netId {cachedNetId}");
                oldSirenState = false;
                IsSirenActive = true;
#if SIREN
                _vehicle.IsSirenActive = true;
#endif
            }

            //if (ELS.GameTime - lastCheck > 100)
            //{
            //    _siren.SetData(State["elssiren"]);
            //    _light.SetData(State["elslight"]);
            //}
            if (distance < reactivateDistance)
            {
                _siren.Ticker();
            }
            else if (distance > deactivateDistance)
            {
                CleanUP(true);
            }

            _light.Ticker();

            if (_siren._mainSiren._enable && _light._stage.CurrentStage != 3 && ELS.GameTime - lastAutoDisable > 1000)
            {
                lastAutoDisable = ELS.GameTime;
                Utils.DeveloperWriteLine("Disabling siren because stage not 3");
                _siren._mainSiren.SetEnable(false);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MainSiren, this, true);
            }
        }

        internal Vector3 GetBonePosistion()
        {
            return _vehicle.Bones["door_dside_f"].Position;
        }

        internal void SyncUi()
        {
            _light.SyncUi();
            _siren.SyncUi();
        }

        internal int GetStage() => _light._stage.CurrentStage;

        int lastExists = 0;
        bool lastExistsValue = false;
        private Vehicle vehicle;

        public override bool Exists()
        {
            if (lastExists == ELS.GameTime)
            {
                return lastExistsValue;
            }
            lastExistsValue = Vehicle.Exists(_vehicle);
            return lastExistsValue;
        }

        public void DisableSiren()
        {
            if (_siren._mainSiren._enable)
            {
                _siren._mainSiren.SetEnable(false);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MainSiren, this, true);
            }
            if (_siren.dual_siren)
            {
                _siren._tones.tone1.SetState(false);
                _siren._tones.tone2.SetState(false);
                _siren._tones.tone3.SetState(false);
                _siren._tones.tone4.SetState(false);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.DualSiren, this, true);
            }
        }

        public override void Delete()
        {
            try
            {

                _light.CleanUP();
                _siren.CleanUP();
                _vehicle.SetExistOnAllMachines(false);
                ELS.TriggerServerEvent("ELS:FullSync:RemoveStale", cachedNetId, true);
                API.SetEntityAsMissionEntity(_vehicle.Handle, true, true);
                VehicleManager.vehicleList.Remove(_vehicle.NetworkId);
                _vehicle.Delete();
            }
            catch (Exception e)
            {
                CitizenFX.Core.Debug.WriteLine($"ELSVehicle.cs:Delete error: {e.Message}");
            }
        }

        readonly Timer lastNetworkTime = new Timer();
        public int NetworkId
        {
            get
            {
                if (cachedNetId == 0 && Vehicle.Exists(_vehicle) && _vehicle.IsNetworked() && lastNetworkTime.Expired)
                {
                    lastNetworkTime.Limit = 2000;
                    var netId = _vehicle.NetworkId;
                    if (API.NetworkDoesNetworkIdExist(netId) && netId != _vehicle.Handle)
                    {
                        cachedNetId = netId;
                    }
                }
                return cachedNetId;
            }
        }

        internal int GetNetworkId()
        {
            return NetworkId;
        }
        /// <summary>
        /// Proxies sync data to the lighting and siren sub components
        /// </summary>
        /// <param name="dataDic"></param>
        public void SetData(IDictionary<string, object> data)
        {
#if STATEBAG
            if (State != null)
            {
                var siren = State["elssiren"] as IDictionary<string, object>;
                _siren.SetData(siren);

                var light = State["elslight"] as IDictionary<string, object>;
                _light.SetData(light);
            }
#else
            if (data.TryGetValue(DataNames.Siren, out var siren))
            {
                _siren.SetData((IDictionary<string, object>)siren);
            }
            if (data.TryGetValue(DataNames.Light, out var light))
            {
                _light.SetData((IDictionary<string, object>)light);
            }
#endif
        }

        public void SetLightData(IDictionary<string, object> data)
        {
            _light.SetData(data);
        }

        public void SetSirenData(IDictionary<string, object> data)
        {
            _siren.SetData(data);
        }

        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> vehDic = new Dictionary<string, object>
            {
                {DataNames.Siren, _siren.GetData() },
                {DataNames.Light, _light.GetData() }
            };
            return vehDic;
        }

        internal void GetSaveSettings()
        {
            UserSettings.ELSUserVehicle veh = new UserSettings.ELSUserVehicle()
            {
                ServerId = ELS.ServerId,
                PrmPatt = _light.CurrentPrmPattern,
                SecPatt = _light.CurrentSecPattern,
                WrnPatt = _light.CurrentWrnPattern,
                Siren = _siren._mainSiren.MainTones[_siren._mainSiren.currentTone].Type,
                Model = _vehicle.Model.Hash
            };
            ELS.userSettings.SaveVehicles(veh);
        }

        internal void SetOutofVeh()
        {
            if (_vcf.PRML.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentPrmPattern = _vcf.PRML.ForcedPatterns.OutOfVeh.IntPattern;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangePrmPatt, NetworkId);
            }
            if (_vcf.SECL.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentSecPattern = _vcf.SECL.ForcedPatterns.OutOfVeh.IntPattern;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangeSecPatt, NetworkId);
            }
            if (_vcf.WRNL.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentWrnPattern = _vcf.WRNL.ForcedPatterns.OutOfVeh.IntPattern;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangeWrnPatt, NetworkId);
            }
        }

        internal void SetInofVeh(bool skipSync = false)
        {
            bool send = false;
            if (_vcf.PRML.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentPrmPattern = _light._oldprm;
                send = true;
            }
            if (_vcf.SECL.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentSecPattern = _light._oldsec;
                send = true;
            }
            if (_vcf.WRNL.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentWrnPattern = _light._oldwrn;
                send = true;
            }
            if (send && !skipSync)
            {
                Utils.DeveloperWriteLine("SyncRequestReply invoked from ELSVehicle::SetInofVehicle 446");
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangePrmPatt, NetworkId);
            }

        }
    }
}