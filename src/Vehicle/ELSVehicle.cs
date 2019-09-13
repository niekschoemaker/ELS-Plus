﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CitizenFX.Core;
using ELS.configuration;
using CitizenFX.Core.Native;
using ELS.Manager;
using System.Threading.Tasks;
using ELS.NUI;

namespace ELS
{
    public class ELSVehicle : PoolObject, FullSync.IFullSyncComponent
    {
        private Siren.Siren _siren;
        private Light.Lights _light;
        private Vehicle _vehicle;
        private Vcfroot _vcf;
        int lastdrivetime;
        internal int cachedNetId;

        public ELSVehicle(int handle, [Optional]IDictionary<string, object> data) : base(handle)
        {
            _vehicle = new Vehicle(handle);
            ModelLoaded();
            lastdrivetime = Game.GameTime;
            API.SetVehRadioStation(_vehicle.Handle, "OFF");
            API.SetVehicleRadioEnabled(_vehicle.Handle, false);
            if (_vehicle.DisplayName == "CARNOTFOUND")
            {
                throw new Exception("ELSVehicle.cs:Vehicle not found");
            }
            else if (_vehicle.GetNetworkId() == 0)
            {
                throw new Exception("ELSVehicle.cs:NetworkId is 0");
            }
            else if (VCF.ELSVehicle.ContainsKey(_vehicle.Model))
            {
                _vcf = VCF.ELSVehicle[_vehicle.Model].root;
            }
            try
            {
                Function.Call((Hash)0x5f3a3574, _vehicle.Handle, true);
            }
            catch (Exception e)
            {
                Utils.ReleaseWriteLine("ELSVehicle.cs:Repair Fix is not enabled on this client");
            }
            _light = new Light.Lights(_vehicle, _vcf, (IDictionary<string, object>)data?["light"]);
            _siren = new Siren.Siren(_vehicle, _vcf, (IDictionary<string, object>)data?["siren"], _light);
            _light.SetGTASirens(false);
            cachedNetId = _vehicle.GetNetworkId();
#if DEBUG
            Utils.DebugWriteLine(API.IsEntityAMissionEntity(_vehicle.Handle).ToString());
            Utils.DebugWriteLine($"ELSVehicle.cs:registering netid:{_vehicle.GetNetworkId()}\n" +
                $"Does entity belong to this script:{CitizenFX.Core.Native.API.DoesEntityBelongToThisScript(_vehicle.Handle, false)}");
            Utils.DebugWriteLine($"ELSVehicle.cs:created vehicle");
#endif
        }
        private async void ModelLoaded()
        {
            while (_vehicle.DisplayName == "CARNOTFOUND")
            {
                await CitizenFX.Core.BaseScript.Delay(0);
            }
        }
        internal void CleanUP()
        {
            _siren.CleanUP();
            _light.CleanUP();
#if DEBUG
            Utils.DebugWriteLine("ELSVehicle.cs:running vehicle deconstructor");
#endif
        }

        internal Vehicle GetVehicle { get { return _vehicle; } }

        internal void RunControlTick()
        {
            if (!_vehicle.Exists() || _vehicle.IsDead)
            {
                VehicleManager.vehicleList.Remove(cachedNetId);
                ELS.TriggerServerEvent("ELS:FullSync:RemoveStale", cachedNetId, _vehicle.IsDead);
                return;
            }
            _siren.Ticker();
            _light.ControlTicker();
        }

        internal void RunTick()
        {
            if (!_vehicle.Exists() || _vehicle.IsDead)
            {
                VehicleManager.vehicleList.Remove(cachedNetId);
                ELS.TriggerServerEvent("ELS:FullSync:RemoveStale", cachedNetId, _vehicle.IsDead);
                return;
            }
            _light.Ticker();
            
            if (_siren._mainSiren._enable && _light._stage.CurrentStage != 3)
            {
                _siren._mainSiren.SetEnable(false);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MainSiren, _vehicle, true, Game.Player.ServerId);
            }
        }

        internal void RunExternalTick()
        {
            if (!_vehicle.Exists() || _vehicle.IsDead)
            {
                VehicleManager.vehicleList.Remove(cachedNetId);
                ELS.TriggerServerEvent("ELS:FullSync:RemoveStale", cachedNetId, _vehicle.IsDead);
                return;
            }            
            _siren.ExternalTicker();
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

        public override bool Exists()
        {
            return CitizenFX.Core.Native.Function.Call<bool>(CitizenFX.Core.Native.Hash.DOES_ENTITY_EXIST, _vehicle);
        }

        public void DisableSiren()
        {
            if (_siren._mainSiren._enable)
            {
                _siren._mainSiren.SetEnable(false);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MainSiren, _vehicle, true, Game.Player.ServerId);
            }
            if (_siren.dual_siren)
            {
                _siren._tones.tone1.SetState(false);
                _siren._tones.tone2.SetState(false);
                _siren._tones.tone3.SetState(false);
                _siren._tones.tone4.SetState(false);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.DualSiren, _vehicle, true, Game.Player.ServerId);
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
                VehicleManager.vehicleList.Remove(_vehicle.GetNetworkId());
                _vehicle.Delete();
            }
            catch (Exception e)
            {
                CitizenFX.Core.Debug.WriteLine($"ELSVehicle.cs:Delete error: {e.Message}");
            }
        }

        internal void UpdateRemoteSiren(string command, bool state)
        {
            _siren.SirenControlsRemote(command, state);
        }

        internal void UpdateRemoteLights()
        {
            _light.LightsControlsRemote();
        }

        internal int GetNetworkId()
        {
            return _vehicle.GetNetworkId();
        }
        /// <summary>
        /// Proxies sync data to the lighting and siren sub components
        /// </summary>
        /// <param name="dataDic"></param>
        public void SetData(IDictionary<string, object> data)
        {
            _siren.SetData((IDictionary<string, object>)data["siren"]);
            _light.SetData((IDictionary<string, object>)data["light"]);
        }

        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> vehDic = new Dictionary<string, object>
            {
                {"siren",_siren.GetData() },
                {"light",_light.GetData() },
                {"NetworkID",_vehicle.GetNetworkId() }
            };
            return vehDic;
        }

        internal void SetSaveSettings(UserSettings.ELSUserVehicle veh)
        {
            _light.CurrentPrmPattern = veh.PrmPatt;
            _light.CurrentSecPattern = veh.SecPatt;
            _light.CurrentWrnPattern = veh.WrnPatt;
            
            VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangePrmPatt, this.GetNetworkId(), Game.Player.ServerId);
            VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangeSecPatt, this.GetNetworkId(), Game.Player.ServerId);
            VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangeWrnPatt, this.GetNetworkId(), Game.Player.ServerId);
            switch(veh.Siren)
            {
                case "WL":
                    _siren._mainSiren.setMainTone(_siren._tones.tone1);
                    break;
                case "YP":
                    _siren._mainSiren.setMainTone(_siren._tones.tone2);
                    break;
                case "A1":
                    _siren._mainSiren.setMainTone(_siren._tones.tone3);
                    break;
                case "A2":
                    _siren._mainSiren.setMainTone(_siren._tones.tone4);
                    break;
            }
            VehicleManager.SyncRequestReply(RemoteEventManager.Commands.MainSiren, this.GetNetworkId(), Game.Player.ServerId);
        }

        internal void GetSaveSettings()
        {
            UserSettings.ELSUserVehicle veh = new UserSettings.ELSUserVehicle()
            {
                ServerId = ELS.ServerId,
                PrmPatt = _light.CurrentPrmPattern,
                SecPatt = _light.CurrentSecPattern,
                WrnPatt = _light.CurrentWrnPattern,
                Siren = _siren._mainSiren.currentTone.Type,
                Model = _vehicle.Model.Hash
            };
            ELS.userSettings.SaveVehicles(veh);
        }

        internal void SetOutofVeh()
        {
            if (_vcf.PRML.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentPrmPattern = _vcf.PRML.ForcedPatterns.OutOfVeh.IntPattern;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangePrmPatt, this.GetNetworkId(), Game.Player.ServerId);
            }
            if (_vcf.SECL.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentSecPattern = _vcf.SECL.ForcedPatterns.OutOfVeh.IntPattern;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangeSecPatt, this.GetNetworkId(), Game.Player.ServerId);
            }
            if (_vcf.WRNL.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentWrnPattern = _vcf.WRNL.ForcedPatterns.OutOfVeh.IntPattern;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangeWrnPatt, this.GetNetworkId(), Game.Player.ServerId);
            }
        }

        internal void SetInofVeh()
        {
            if (_vcf.PRML.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentPrmPattern = _light._oldprm;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangePrmPatt, this.GetNetworkId(), Game.Player.ServerId);
            }
            if (_vcf.SECL.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentSecPattern = _light._oldsec;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangeSecPatt, this.GetNetworkId(), Game.Player.ServerId);
            }
            if (_vcf.WRNL.ForcedPatterns.OutOfVeh.Enabled)
            {
                _light.CurrentWrnPattern = _light._oldwrn;
                VehicleManager.SyncRequestReply(RemoteEventManager.Commands.ChangeWrnPatt, this.GetNetworkId(), Game.Player.ServerId);
            }
        }
    }
}