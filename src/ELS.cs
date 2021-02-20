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
#define SIREN
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using ELS.configuration;
using ELS.Light;
using ELS.Manager;
using ELS.NUI;
using ELSShared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Control = CitizenFX.Core.Control;

namespace ELS
{
    public class ELS : BaseScript
    {
        public static int GameTime { get; private set; }
        internal static int? CurrentSeat { get; private set; }
        internal static Vehicle CurrentVehicle { get; private set; }
        private readonly FileLoader _FileLoader;
        private SpotLight _spotLight;
        private ElsConfiguration _controlConfiguration;
        private bool _firstTick = false;
        internal static string ServerId;
        internal static UserSettings userSettings;
        public static bool Loaded { get; set; }

        public ELS()
        {
            bool Loaded = false;
            _controlConfiguration = new ElsConfiguration();
            _FileLoader = new FileLoader(this);
            EventHandlers["onClientResourceStart"] += new Action<string>(async (string obj) =>
                {
                    if (obj == CurrentResourceName())
                    {
                        Utils.Debug = bool.Parse(API.GetConvar("elsplus_debug", "false"));
                        try
                        {
                            await Delay(100);
                            ServerId = API.GetConvar("ElsServerId", null);
                            userSettings = new UserSettings();
                            await Delay(0);
                            Task settingsTask = new Task(() => userSettings.LoadUserSettings());
                            await Delay(0);
                            Utils.ReleaseWriteLine($"Welcome to ELS Plus on {ServerId} using version {Assembly.GetExecutingAssembly().GetName().Version}");
                            settingsTask.Start();
                            await Delay(250);
                            _FileLoader.RunLoader(obj);
                            await Delay(250);
                            SetupConnections();
                            await Delay(250);
                            TriggerServerEvent(EventNames.VcfSyncServer, Game.Player.ServerId);
                            await Delay(250);
                            Tick -= Class1_Tick;
                            Tick += Class1_Tick;
                            await Delay(1000);
                            if (float.TryParse(API.GetResourceKvpString("daybrightness"), out var dayValue))
                            {
                                Function.Call((Hash)3520272001, "car.defaultlight.day.emissive.on", dayValue);
                            }
                            else
                            {
                                Function.Call((Hash)3520272001, "car.defaultlight.day.emissive.on", Global.DayLtBrightness);
                            }
                            if (float.TryParse(API.GetResourceKvpString("nightbrightness"), out var nightValue))
                            {
                                Function.Call((Hash)3520272001, "car.defaultlight.night.emissive.on", nightValue);
                            }
                            else
                            {
                                Function.Call((Hash)3520272001, "car.defaultlight.night.emissive.on", Global.NightLtBrightness);
                            }

                            API.RequestScriptAudioBank("DLC_WMSIRENS\\SIRENPACK_ONE", false);

                        }
                        catch (Exception e)
                        {
                            Screen.ShowNotification($"ERROR:{e.Message}");
                            Screen.ShowNotification($"ERROR:{e.StackTrace}");
                            Tick -= Class1_Tick;
                            Utils.ReleaseWriteLine($"ERROR:{e.StackTrace}");
                            Utils.ReleaseWriteLine($"Error: {e.Message}");
                        }
                    }
                });
            if (API.GetConvar("dev_server", "false") == "true")
            {
                Utils.IsDeveloper = true;
            }

        }
        int lastVehicle = -1;
        private void SetupConnections()
        {
            API.RegisterKeyMapping("togglehazard", "Alarm lichten", "keyboard", "");
            API.RegisterKeyMapping("toggleleftblinker", "Linker knipperlicht", "keyboard", "MINUS");
            API.RegisterKeyMapping("togglerightblinker", "Rechts knipperlicht", "keyboard", "EQUALS");
            EventHandlers["onClientResourceStop"] += new Action<string>(async (string obj) =>
            {
                if (obj == Function.Call<string>(Hash.GET_CURRENT_RESOURCE_NAME))
                {
                    VehicleManager.Instance.CleanUP();
                    _FileLoader.UnLoadFilesFromScript(obj);
                }
            });

            EventHandlers[EventNames.VcfSyncClient] += new Action<List<object>>((vcfs) =>
            {
                VCF.ParseVcfs(vcfs);
            });

            EventHandlers.Add("ELS:PatternSync:Client", new Action<List<dynamic>>((patterns) =>
            {
                VCF.ParsePatterns(patterns);
            }));
            EventHandlers["ELS:FullSync:NewSpawnWithData"] += new Action<IDictionary<string, object>>((a) =>
            {
                VehicleManager.Instance.SyncAllVehiclesOnFirstSpawn(a);
            });
        }

        [EventHandler("baseevents:leftVehicle")]
        public void LeftVehicle()
        {
            CurrentSeat = null;
            CurrentVehicle = null;
        }

        [EventHandler("baseevents:enteredVehicle")]
        public async void EnteredVehicle(int veh, int currentSeat, dynamic _, int netId)
        {
            CurrentSeat = currentSeat;
            CurrentVehicle = new Vehicle(veh);
            //Utils.DebugWriteLine($"Vehicle with model {model} entered checking list. displayName: {displayName}; currentSeat: {currentSeat}; veh: {veh}; netId: {netId}");
            Vehicle vehicle = CurrentVehicle;
            await Delay(1000);
            try
            {
                if (Vehicle.Exists(vehicle) && vehicle.IsEls())
                {
                    if (netId == Game.PlayerPed.CurrentVehicle.NetworkId)
                    {
                        lastVehicle = netId;
                        if (userSettings.uiSettings.enabled)
                        {
                            ElsUiPanel.ShowUI();
                        }

                        VehicleManager.SyncUI(netId);

                        if (VehicleManager.vehicleList.TryGetValue(netId, out var elsVehicle))
                        {
                            elsVehicle.SetInofVeh(true);
                        }

                        VehicleManager.SyncRequestReply(RemoteEventManager.Commands.FullSync, netId);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.ReleaseWriteLine("Exception caught via vehicle entered");
                Utils.ReleaseWriteLine(e.ToString());
            }
        }

        [EventHandler(EventNames.NewLightSyncData)]
        public async void SetVehicleLightData(IDictionary<string, object> dataDic, int NetworkId, int PlayerId)
        {
            while (!Loaded)
            {
                await Delay(0);
            }
            VehicleManager.Instance.SetVehicleLightData(dataDic, NetworkId, PlayerId);
        }

        [EventHandler(EventNames.NewSirenSyncData)]
        public async void SetVehicleSirenData(IDictionary<string, object> dataDic, int NetworkId, int PlayerId)
        {
            while (!Loaded)
            {
                await Delay(0);
            }
            VehicleManager.Instance.SetVehicleSirenData(dataDic, NetworkId, PlayerId);
        }

        [EventHandler(EventNames.NewFullSyncData)]
        public async void SetVehicleFullSyncData(IDictionary<string, object> dataDic, int networkId, int PlayerId)
        {
            try
            {
                while (!Loaded)
                {
                    await Delay(0);
                }
                VehicleManager.Instance.SetVehicleSyncData(dataDic, networkId, PlayerId);
            }
            catch (Exception e)
            {
                Utils.ReleaseWriteLine(e.ToString());
            }
            
        }

        [EventHandler(EventNames.FullSyncNewSpawn)]
        public void NewSpawn()
        {
            Tick -= Class1_Tick;
            Tick += Class1_Tick;
        }

        [EventHandler("playerSpawned")]
        public void PlayerSpawned()
        {
            TriggerServerEvent(EventNames.FullSycnRequestAll, Game.Player.ServerId);
        }

        [EventHandler(EventNames.RemoveStale)]
        public void RemoveStale(int netId, bool dead)
        {
            if (VehicleManager.vehicleList.TryGetValue(netId, out var eLSVehicle))
            {
                Utils.ReleaseWriteLine($"{netId} was removed. Reason: doesn't exist.");
                eLSVehicle.CleanUP();
                VehicleManager.vehicleList.Remove(netId);
            }
            else
            {
                Utils.ReleaseWriteLine($"{netId} wasn't known to this client.");
            }
        }

        [EventHandler(EventNames.VehicleExited)]
        public void VehicleExited(int veh, int netId)
        {
            Vehicle vehicle = new Vehicle(veh);
            try
            {
                if (vehicle != null && vehicle.IsEls())
                {
                    if (netId == vehicle.Handle)
                    {
                        netId = vehicle.NetworkId;
                    }
                    if (VehicleManager.vehicleList.ContainsKey(netId) && (netId == lastVehicle))
                    {
                        if (Global.DisableSirenOnExit)
                        {
                            VehicleManager.vehicleList[netId].DisableSiren();
                        }

                        VehicleManager.vehicleList[netId].SetOutofVeh();
                    }
                }
            }
            catch (Exception e)
            {
                Utils.ReleaseWriteLine("Exception caught via vehicle exited");
            }
        }

        [Command("els_data")]
        public void GetData(string[] args)
        {
            int number;
            if (args.Length < 1 || !int.TryParse(args[0], out number))
            {
                Debug.WriteLine("Invalid args");
                return;
            }

            if (!VehicleManager.vehicleList.TryGetValue(number, out var elsVehicle))
            {
                Debug.WriteLine($"Voertuig met id {number} niet gevonden in els lijst!");
                return;
            }

            Debug.WriteLine(JsonConvert.SerializeObject(elsVehicle.GetData()));
        }

        [Command("els_developer")]
        public void SetDeveloper()
        {
            Utils.IsDeveloper = !Utils.IsDeveloper;
            Utils.ReleaseWriteLine($"Debug logging is now { (Utils.IsDeveloper ? "enabled" : "disabled") }");
        }

        [Command("carlist")]
        public void CarList()
        {
            var sb = new StringBuilder("Cars currently registered are: \n");
            foreach (var car in VehicleManager.vehicleList)
            {
                var stage = car.Value.GetStage();
                var vehicle = car.Value.Vehicle;
                sb.AppendLine($"{vehicle?.Handle} ({car.Value.cachedNetId}:{car.Key}:{car.Value.NetworkId}) : stage: {stage}");
            }
            Utils.ReleaseWriteLine(sb.ToString());
        }

        [Command("elsdaybrightness")]
        public void ElsDayBrightness(string[] args)
        {
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]?.ToString()))
            {
                return;
            }

            if (float.TryParse(args[0].ToString(), out var value))
            {
                API.SetResourceKvp("daybrightness", value.ToString());
                Function.Call((Hash)3520272001, "car.defaultlight.day.emissive.on", value);
            }
        }

        [Command("elsnightbrightness")]
        public void ElsNightBrightness(string[] args)
        {
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]?.ToString()))
            {
                return;
            }

            if (float.TryParse(args[0].ToString(), out var value))
            {
                API.SetResourceKvp("nightbrightness", value.ToString());
                Function.Call((Hash)3520272001, "car.defaultlight.night.emissive.on", value);
            }
        }

        [Command("elslist")]
        public void ElsList()
        {
            var listString = "";
            listString += $"Available ELS Vehicles\n" +
                          $"----------------------\n";
            foreach (var entry in VCF.ELSVehicle.Values)
            {
                if (entry.modelHash.IsValid)
                {
                    listString += $"{System.IO.Path.GetFileNameWithoutExtension(entry.filename)}\n";
                }
            }
            CitizenFX.Core.Debug.WriteLine(listString);
        }

        int lastCall = 0;
        private static Player player;
        private static Ped ped;
        private static Vector3? position;

#if STATEBAG
        [Command("els_getstate")]
        public void GetState(int source, List<object> args, string raw)
        {
            try
            {
                if (VehicleManager.vehicleList.TryGetValue(CurrentVehicle.NetworkId, out var currentVehicle))
                {
                    var elsLight = currentVehicle.State["elslight"] as IDictionary<string, object>;
                    var elsSiren = currentVehicle.State["elssiren"] as IDictionary<string, object>;
                    Debug.WriteLine(JsonConvert.SerializeObject(elsLight));
                    Debug.WriteLine(JsonConvert.SerializeObject(elsSiren));
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
                Console.WriteLine(e);
            }
        }
#endif

        [Command("vcfsync")]
        public void VcfSync()
        {
            if (GameTime - lastCall < 60000)
            {
                return;
            }
            lastCall = GameTime;
            TriggerServerEvent(EventNames.VcfSyncServer, Game.Player.ServerId);
        }

        [Command("elsui")]
        public void ElsUI(string[] args)
        {
            if (args.Length != 1)
            {
                return;
            }
            Task task = new Task(() => userSettings.SaveUI(true));
            switch (args[0].ToString().ToLower())
            {
                case "enable":
                    ElsUiPanel.EnableUI();
                    break;
                case "disable":
                    ElsUiPanel.DisableUI();
                    userSettings.uiSettings.enabled = false;
                    break;
                case "show":
                    ElsUiPanel.ShowUI();
                    userSettings.uiSettings.enabled = true;
                    break;
                case "whelen":
                    userSettings.uiSettings.currentUI = UserSettings.ElsUi.Whelen;
                    break;
                case "new":
                    userSettings.uiSettings.currentUI = UserSettings.ElsUi.NewHotness;
                    break;
                case "old":
                    userSettings.uiSettings.currentUI = UserSettings.ElsUi.OldandBusted;
                    break;
            }
            task.Start();
        }

        public static string CurrentResourceName()
        {
            return Function.Call<string>(Hash.GET_CURRENT_RESOURCE_NAME);
        }

#region Callbacks for GUI
        public void RegisterNUICallback(string msg, Func<IDictionary<string, object>, CallbackDelegate, CallbackDelegate> callback)
        {
            CitizenFX.Core.Debug.WriteLine($"Registering NUI EventHandler for {msg}");
            API.RegisterNuiCallbackType(msg);
            // Remember API calls must be executed on the first tick at the earliest!
            //Function.Call(Hash.REGISTER_NUI_CALLBACK_TYPE, msg);
            EventHandlers[$"__cfx_nui:{msg}"] += new Action<ExpandoObject, CallbackDelegate>((body, resultCallback) =>
            {
                Console.WriteLine("We has event" + body);
                callback.Invoke(body, resultCallback);
            });
            EventHandlers[$"{msg}"] += new Action<ExpandoObject, CallbackDelegate>((body, resultCallback) =>
            {
                Console.WriteLine("We has event without __cfx_nui" + body);
                callback.Invoke(body, resultCallback);
            });

        }
#endregion

        internal static Player Player
        {
            get {
                if (player == null)
                {
                    player = new Player(API.PlayerId());
                }
                return player;
            }
            private set => player = value; }
        internal static Ped Ped
        {
            get
            {
                if (ped == null)
                {
                    ped = new Ped(API.PlayerPedId());
                }
                return ped;
            }
            private set => ped = value;
        }
        internal static Vector3 Position {
            get
            {
                if (position == null)
                {
                    position = Ped.Position;
                }
                return position.GetValueOrDefault();
            }
            set => position = value;
        }
        private async Task Class1_Tick()
        {
            try
            {
                if (!_firstTick)
                {
                    RegisterNUICallback("escape", ElsUiPanel.EscapeUI);
                    RegisterNUICallback("togglePrimary", ElsUiPanel.TooglePrimary);
                    RegisterNUICallback("keyPress", ElsUiPanel.KeyPress);
                    _firstTick = true;
                }
                if (!Loaded)
                {
                    return;
                }
                GameTime = Game.GameTime;
                Player = null;
                Ped = null;
                position = null;
                VehicleManager.Instance.RunTick();
                if (CurrentVehicle?.IsEls() ?? false)
                {
                    Game.DisableControlThisFrame(0, Control.FrontendPause);
                    if (Game.IsControlPressed(0, Control.VehicleMoveUpDown) && Game.IsDisabledControlJustReleased(0, Control.FrontendPause))
                    {

                        if (ElsUiPanel._enabled == 0)
                        {
                            ElsUiPanel.ShowUI();
                            userSettings.uiSettings.enabled = true;
                        }
                        else if (ElsUiPanel._enabled == 1)
                        {
                            ElsUiPanel.DisableUI();
                            userSettings.uiSettings.enabled = false;
                        }
                        Task task = new Task(() => userSettings.SaveUI(false));
                        task.Start();
                    }
                }
                else
                {
                    if (ElsUiPanel._enabled != 0)
                    {
                        ElsUiPanel.DisableUI();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ReleaseWriteLine($"ERROR {ex}");
            }
        }
    }
}

