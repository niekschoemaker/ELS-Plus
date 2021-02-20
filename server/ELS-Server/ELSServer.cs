using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Reflection;
using ELSShared;
using Newtonsoft.Json;
using Shared;
using System.Dynamic;
using System.Linq;
using System.Diagnostics;

namespace ELS_Server
{
    public class ELSServer : BaseScript
    {
        Dictionary<int, IDictionary<string, object>> _cachedData = new Dictionary<int, IDictionary<string, object>>();
        Dictionary<int, string> _lastPlayerData = new Dictionary<int, string>();
        string serverId;
        string currentVersion;
        public ELSServer()
        {
            currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Utils.ReleaseWriteLine($"Welcome to ELS+ {currentVersion} for FiveM");
            serverId = API.LoadResourceFile(API.GetCurrentResourceName(), "ELSId");

            if (string.IsNullOrEmpty(serverId))
            {
                Guid uuid = Guid.NewGuid();
                API.SaveResourceFile(API.GetCurrentResourceName(), "ELSId", uuid.ToString(), -1);
                serverId = uuid.ToString();
            }

            foreach (string s in Configuration.ElsVehicleGroups)
            {
                API.ExecuteCommand($"add_ace group.{s} command.elscar allow");
            }

            API.RegisterCommand("vcfrefresh", new Action<int, List<object>, string>((source, arguments, raw) =>
            {
                Utils.ReleaseWriteLine($"{Players[source].Name} has activated a VCF Refresh");
                foreach (Player p in Players)
                {
                    TriggerEvent(EventNames.VcfSyncServer, p.Handle);
                }
            }), true);

            EventHandlers[EventNames.RemoveStale] += new Action<int, bool>(async (int netid, bool dead) =>
            {
                var vehicle = API.NetworkGetEntityFromNetworkId(netid);
                if (!API.DoesEntityExist(vehicle))
                {
                    Utils.ReleaseWriteLine($"{netid} was removed. Reason: doesn't exist.");
                    _cachedData.Remove(netid);
                    _lastPlayerData.Remove(netid);

                    TriggerClientEvent(EventNames.RemoveStale, netid);
                }
                else
                {
                    Utils.ReleaseWriteLine($"{netid} wasn't removed. Reason: still exists.");
                }
            });

            EventHandlers["baseevents:leftVehicle"] += new Action<int, int, string, int>((veh, seat, name, netId) =>
            {
                if (_cachedData.ContainsKey(netId))
                {
                    TriggerClientEvent(EventNames.VehicleExited, veh, netId);
                }
            });

            API.RegisterCommand("resync", new Action<int, System.Collections.IList, string>((a, b, c) =>
            {
                TriggerClientEvent(Players[(int.Parse((string)b[0]))], EventNames.NewSpawnWithData, _cachedData);
            }), true);

            API.RegisterCommand("clearcache", new Action<int, System.Collections.IList, string>((a, b, c) =>
            {
                _cachedData.Clear();

                _lastPlayerData.Clear();
                Utils.ReleaseWriteLine("ELS Cache cleared");
            }), true);

            Task task = new Task(() => PreloadSyncData());
            task.Start();
            Tick += Server_Tick;

            try
            {
                API.SetConvarReplicated("ELSServerId", serverId);
            }
            catch (Exception e)
            {
                Utils.ReleaseWriteLine("Please update your CitizenFX server to the latest artifact version to enable update check and server tracking");
            }
        }

        public IDictionary<string, object> GetData(int networkID)
        {
            if (_cachedData.TryGetValue(networkID, out var data)) {
                return data;
            }

            return null;
        }

        [Conditional("STATEBAG")]
        public void UpdateState(int networkID, IDictionary<string, object> data = null)
        {
            data = data ?? GetData(networkID);
            if (data == null)
            {
                return;
            }

            var vehicle = new Vehicle(API.NetworkGetEntityFromNetworkId(networkID));
            foreach (var p in data)
            {
                vehicle.State["els" + p.Key] = p.Value;
            }
        }

        [EventHandler(EventNames.VcfSyncServer)]
        public void VcfSyncServer([FromSource] Player player)
        {
            TriggerClientEvent(player, EventNames.VcfSyncClient, VcfSync.VcfData);
            TriggerClientEvent(player, "ELS:PatternSync:Client", CustomPatterns.Patterns);
        }

        [EventHandler(EventNames.FullSyncBroadcast)]
        public void FullSyncBroadcast([FromSource] Player player, ExpandoObject expandoData, int networkId)
        {
            var dataDict = (IDictionary<string, object>)expandoData;
            _cachedData[networkId] = dataDict;
            CheckModel(networkId);
            _lastPlayerData[networkId] = player.Handle;

            UpdateState(networkId, dataDict);
            TriggerClientEvent(EventNames.NewFullSyncData, expandoData, networkId, int.Parse(player.Handle));
        }

        [EventHandler(EventNames.FullSycnRequestAll)]
        public void FullSyncRequestAll([FromSource] Player player)
        {
            TriggerClientEvent(player, EventNames.NewSpawnWithData, _cachedData);
        }

        [EventHandler(EventNames.FullSyncRequestOne)]
        public void FullSyncRequestOne([FromSource] Player player, int networkID)
        {
            if (_cachedData.TryGetValue(networkID, out var data))
            {
                if (_lastPlayerData.TryGetValue(networkID, out var player1)) {
                    TriggerClientEvent(player, EventNames.NewFullSyncData, data, networkID, int.Parse(player1));
                }
            }
        }

        [EventHandler(EventNames.LightSyncBroadcast)]
        public void LightSyncBroadcast([FromSource] Player player, IDictionary<string, object> newData, int networkID)
        {
            try
            {
                if (!_cachedData.TryGetValue(networkID, out var cachedData))
                {
                    cachedData = new Dictionary<string, object>();
                    _cachedData.Add(networkID, cachedData);
                }

                CheckModel(networkID);

                if (!cachedData.TryGetValue(DataNames.Light, out var light))
                {
                    light = new Dictionary<string, object>();
                    cachedData.Add(DataNames.Light, light);
                }

                var lightDict = light as IDictionary<string, object>;

                foreach (var a in newData)
                {
                    lightDict[a.Key] = a.Value;
                }

                _lastPlayerData[networkID] = player.Handle;

                UpdateState(networkID, cachedData);
                TriggerClientEvent(EventNames.NewLightSyncData, newData, networkID, int.Parse(player.Handle));
            }
            catch (Exception e)
            {
                var resource = API.GetCurrentResourceName();
                Utils.ReleaseWriteLine(e.ToString());
                TriggerEvent("SentryIO-Server:Error", int.Parse(player.Handle), e.Message, e.StackTrace, resource, new { resource = resource, traceback = e.ToString() });
            }
        }

        public void CheckModel(int networkId)
        {
            var cachedData = _cachedData[networkId];
            if (!cachedData.ContainsKey(DataNames.Model))
            {
                var vehicle = new Vehicle(API.NetworkGetEntityFromNetworkId(networkId));
                cachedData[DataNames.Model] = vehicle.Model;
            }
        }

        [Command("test_siren")]
        public void TestSirenCommand(Player player, string[] args)
        {
            if (API.GetConvar("dev_server", "false") != "true")
            {
                return;
            }
            var vehicle = (Vehicle)Entity.FromHandle(API.GetVehiclePedIsIn(player.Character.Handle, true));
            TriggerClientEvent(EventNames.NewSirenSyncData, new Dictionary<string, object> {
                { "", null }
            }, vehicle.NetworkId, 0);
        }


        [EventHandler(EventNames.SirenSyncBroadcast)]
        public void SirenSyncBroadCast([FromSource] Player player, IDictionary<string, object> newData, int networkID)
        {
            try
            {
                // Make sure the cachedData exists and otherwise create it
                if (!_cachedData.TryGetValue(networkID, out var cachedData))
                {
                    cachedData = new Dictionary<string, object>();
                    _cachedData.Add(networkID, cachedData);
                }
                CheckModel(networkID);

                if (!cachedData.TryGetValue(DataNames.Siren, out var siren))
                {
                    siren = new Dictionary<string, object>();
                    cachedData.Add(DataNames.Siren, siren);
                }
                var sirenDict = (IDictionary<string, object>)siren;
                foreach (var a in newData)
                {
                    sirenDict[a.Key] = a.Value;
                }
                _lastPlayerData[networkID] = player.Handle;

                UpdateState(networkID, cachedData);
                TriggerClientEvent(EventNames.NewSirenSyncData, newData, networkID, int.Parse(player.Handle));
            }
            catch (Exception e)
            {
                Utils.ReleaseWriteLine(e.ToString());
            }
        }

        async Task PreloadSyncData()
        {
            await VcfSync.CheckResources();
            await CustomPatterns.CheckCustomPatterns();
        }

        void BroadcastMessage(System.Dynamic.ExpandoObject dataDic, int SourcePlayerID)
        {
            TriggerClientEvent(EventNames.NewFullSyncData, dataDic, SourcePlayerID);
        }

        private async Task Server_Tick()
        {
            foreach (var a in _cachedData.Keys.ToList())
            {
                await Delay(0);
                var _vehicle = API.NetworkGetEntityFromNetworkId(a);
                if (!API.DoesEntityExist(_vehicle))
                {
                    Utils.ReleaseWriteLine($"Removing netId: {a}");
                    TriggerClientEvent(EventNames.RemoveStale, a);
                    _cachedData.Remove(a);
                }
            }
            await Delay(30000);
        }
    }
}
