﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core.Native;
using ELS.NUI;
using Newtonsoft.Json.Linq;

namespace ELS.configuration
{


    internal class UserSettings
    {

        internal struct ELSUserVehicle
        {
            internal int Model;
            internal string ServerId;
            internal int PrmPatt;
            internal int SecPatt;
            internal int WrnPatt;
            internal string Siren;
        }

        internal enum ElsUi
        {
            NewHotness,
            OldandBusted,
            Whelen
        }

        internal struct UISettings
        {
            internal bool enabled;
            internal ElsUi currentUI;
        }

        static Dictionary<string, ElsUi> ELSUiDic = new Dictionary<string, ElsUi>()
        {
            {"NewHotness", ElsUi.NewHotness },
            {"OldandBusted", ElsUi.OldandBusted },
            {"Whelen", ElsUi.Whelen }
        };

        internal List<ELSUserVehicle> savedVehicles = new List<ELSUserVehicle>();
        internal UISettings uiSettings = new UISettings()
        {
            currentUI = ElsUi.NewHotness,
            enabled = false
        };

        internal async void LoadUserSettings()
        {
            string vehs = API.GetResourceKvpString("elsplus_savedvehicles");
            string uiSet = API.GetResourceKvpString("elsplus_uisettings");
#if DEBUG
            Utils.DebugWriteLine($"UserSettings: got ui settings as: {uiSet}");
            Utils.DebugWriteLine($"UserSettings: got vehicle settings as: {vehs}");
#endif
            if (!String.IsNullOrEmpty(vehs))
            {
                JArray vehArray = JArray.Parse(vehs);
                foreach (JToken obj in vehArray)
                {
                    savedVehicles.Add(new ELSUserVehicle
                    {
                        Model = (int)obj["Model"],
                        ServerId = (string)obj["ServerId"],
                        PrmPatt = (int)obj["PrmPatt"],
                        SecPatt = (int)obj["SecPatt"],
                        WrnPatt = (int)obj["WrnPatt"],
                        Siren = (string)obj["Siren"]
                    });
                }

            }
            if (!String.IsNullOrEmpty(uiSet))
            {
                JObject ui = JObject.Parse(uiSet);
                uiSettings.currentUI = ELSUiDic[(string)ui["currentUI"]];
                uiSettings.enabled = (bool)ui["enabled"];
            }
            ElsUiPanel.InitData();
            ElsUiPanel.DisableUI();
        }

        internal async void SaveVehicles(ELSUserVehicle veh)
        {
            int usrVeh = savedVehicles.FindIndex(uveh => uveh.Model == veh.Model && veh.ServerId == uveh.ServerId);
            if (usrVeh != -1)
            {
                savedVehicles[usrVeh] = veh;
            }
            else
            {
                savedVehicles.Add(veh);
            }
            string vehicles = JsonConvert.SerializeObject(savedVehicles);
            JArray vehArray = new JArray();
            foreach (ELSUserVehicle vehicle in savedVehicles)
            {
                JObject obj = new JObject();
                obj.Add("Model", vehicle.Model);
                obj.Add("ServerId", vehicle.ServerId);
                obj.Add("PrmPatt", vehicle.PrmPatt);
                obj.Add("SecPatt", vehicle.SecPatt);
                obj.Add("WrnPatt", vehicle.WrnPatt);
                obj.Add("Siren", vehicle.Siren);
                vehArray.Add(obj);
            }
#if DEBUG
            Utils.DebugWriteLine($"UserSettings: vehicles json: {vehArray}");
#endif
            API.SetResourceKvp("elsplus_savedvehicles", vehArray.ToString());
#if DEBUG
            Utils.DebugWriteLine($"UserSettings: Vehicle Settings Saved: {API.GetResourceKvpString("elsplus_savedvehicles")}");
#endif
        }

        internal async void SaveUI(bool reload)
        {
            JObject obj = new JObject();
            obj.Add("currentUI", uiSettings.currentUI.ToString());
            obj.Add("enabled", uiSettings.enabled);
            //string uiSet = JsonConvert.SerializeObject(uiSettings);
#if DEBUG
            Utils.DebugWriteLine($"UserSettings: ui json: {obj}");
#endif
            API.SetResourceKvp("elsplus_uisettings", obj.ToString());
#if DEBUG
            Utils.DebugWriteLine($"UserSettings: Ui Settings Saved: {API.GetResourceKvpString("elsplus_uisettings")}");
#endif
            if (reload)
            {
                ElsUiPanel.ReloadUI();
            }
        }
    }
}
