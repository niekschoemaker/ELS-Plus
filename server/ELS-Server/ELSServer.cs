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

namespace ELS_Server
{
    public class ELSServer : BaseScript
    {
        Dictionary<int, IDictionary<string, object>> _cachedData = new Dictionary<int, IDictionary<string, object>>();
        Dictionary<int, Player> _lastPlayerData = new Dictionary<int, Player>();
        long GameTimer;
        long RemoveTimer;
        string serverId;
        string currentVersion;
        public ELSServer()
        {
            currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Utils.ReleaseWriteLine($"Welcome to ELS+ {currentVersion} for FiveM");
            GameTimer = API.GetGameTimer();
            Utils.ReleaseWriteLine($"Setting Game time is {GameTimer}");
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

            EventHandlers[EventNames.VcfSyncServer] += new Action<int>(async (int source) =>
            {
                // Let niet op deze event naam, ehhhh, iets met vage glitch met string naam of zo, this works
                TriggerClientEvent(Players[source], EventNames.VcfSyncClient, VcfSync.VcfData);
                // Triggering Latent twice in one tick appears to cause problems with event name
                await Delay(1000);
                TriggerClientEvent(Players[source], "ELS:PatternSync:Client", CustomPatterns.Patterns);

            });

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

        [EventHandler("ELS:ONDEBUG")]
        public void OnDebugMessage([FromSource] Player player, string message)
        {
            Utils.ReleaseWriteLine(message);
        }

        [EventHandler(EventNames.FullSyncBroadcast)]
        public void FullSyncBroadcast([FromSource] Player player, ExpandoObject expandoData, int networkId)
        {
            var dataDict = (IDictionary<string, object>)expandoData;
            _cachedData[networkId] = dataDict;
            _lastPlayerData[networkId] = player;

            TriggerClientEvent(EventNames.NewFullSyncData, expandoData, networkId, int.Parse(player.Handle));
            var vehicle = new Vehicle(API.NetworkGetEntityFromNetworkId(networkId));
            vehicle.State["els"] = new { applied = true };
        }

        [EventHandler(EventNames.FullSycnRequestAll)]
        public void FullSyncRequestAll([FromSource] Player player)
        {
            TriggerClientEvent(player, EventNames.NewSpawnWithData, _cachedData);
        }

        [EventHandler(EventNames.FullSyncRequestOne)]
        public void FullSyncRequestOne([FromSource] Player player, int networkId)
        {
            if (_cachedData.TryGetValue(networkId, out var data))
            {
                if (_lastPlayerData.TryGetValue(networkId, out var player1)) {
                    TriggerClientEvent(player, EventNames.NewFullSyncData, data, networkId, int.Parse(player1.Handle));
                }
            }
        }

        [EventHandler(EventNames.LightSyncBroadcast)]
        public void LightSyncBroadcast([FromSource] Player player, ExpandoObject expandoData, int NetworkID)
        {
            try
            {
                var dataDict = (IDictionary<string, object>)expandoData;
                if (!_cachedData.TryGetValue(NetworkID, out var data))
                {
                    data = new Dictionary<string, object>()
                        {
                            { DataNames.Light, dataDict }
                        };
                    _cachedData.Add(NetworkID, data);
                }

                if (!data.TryGetValue(DataNames.Light, out var light))
                {
                    light = new Dictionary<string, object>();
                    data.Add(DataNames.Light, light);
                }

                var lightDict = light as IDictionary<string, object>;

                foreach (var a in expandoData)
                {
                    lightDict[a.Key] = a.Value;
                }
                _lastPlayerData[NetworkID] = player;

                TriggerClientEvent(EventNames.NewLightSyncData, expandoData, NetworkID, int.Parse(player.Handle));
                var vehicle = new Vehicle(API.NetworkGetEntityFromNetworkId(NetworkID));
                vehicle.State["els"] = new { applied = true };
            }
            catch (Exception e)
            {
                var resource = API.GetCurrentResourceName();
                Utils.ReleaseWriteLine(e.ToString());
                TriggerEvent("SentryIO-Server:Error", int.Parse(player.Handle), e.Message, e.StackTrace, resource, new { resource = resource, traceback = e.ToString() });
            }
        }

        [EventHandler(EventNames.SirenSyncBroadcast)]
        public void SirenSyncBroadCast([FromSource] Player player, ExpandoObject expandoData, int NetworkID)
        {
            try
            {
                var dataDict = (IDictionary<string, object>)expandoData;

                // Make sure the cachedData exists and otherwise create it
                if (!_cachedData.TryGetValue(NetworkID, out var data))
                {
                    data = new Dictionary<string, object>()
                        {
                            { DataNames.Siren, dataDict }
                        };
                    _cachedData.Add(NetworkID, data);
                }

                if (!data.TryGetValue(DataNames.Siren, out var siren))
                {
                    siren = new Dictionary<string, object>();
                    data.Add(DataNames.Siren, siren);
                }
                var sirenDict = (IDictionary<string, object>)siren;
                foreach (var a in expandoData)
                {
                    sirenDict[a.Key] = a.Value;
                }
                _lastPlayerData[NetworkID] = player;

                TriggerClientEvent(EventNames.NewSirenSyncData, expandoData, NetworkID, int.Parse(player.Handle));
                var vehicle = new Vehicle(API.NetworkGetEntityFromNetworkId(NetworkID));
                vehicle.State["els"] = new { applied = true };
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
            var time = API.GetGameTimer();
            if (time >= RemoveTimer + 10000)
            {
                RemoveTimer = API.GetGameTimer();
                var removeList = new List<int>();
                foreach (var a in _cachedData)
                {
                    var _vehicle = API.NetworkGetEntityFromNetworkId(a.Key);
                    if (!API.DoesEntityExist(_vehicle))
                    {
                        removeList.Add(a.Key);
                    }
                }
                foreach (var a in removeList)
                {
                    Utils.ReleaseWriteLine($"Removing netId: {a}");
                    TriggerClientEvent(EventNames.RemoveStale, a);
                    _cachedData.Remove(a);
                }
            }
        }
    }
}
