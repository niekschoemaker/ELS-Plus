﻿using ELS.FullSync;
using Shared;
using System;
using System.Collections.Generic;

namespace ELS.Light
{
    partial class Lights : IFullSyncComponent
    {
        public Dictionary<string, object> GetData()
        {

            var dic = new Dictionary<string, object>();
            var prm = GetPrimaryLightsData();
            var sec = GetSecondaryLightsData();
            var wrn = GetWarningLightsData();

            if (prm != null && prm.Count > 0)
            {
                dic.Add(DataNames.PRML, prm);
            }
            if (sec != null && sec.Count > 0)
            {
                dic.Add(DataNames.SECL, sec);
            }
            if (wrn != null && wrn.Count > 0)
            {
                dic.Add(DataNames.WRNL, wrn);
            }
            if (_extras.SteadyBurn != null)
            {
                dic.Add(DataNames.SteadyBurn, _extras.SteadyBurn.GetData());
            }
            if (_extras.SceneLights != null)
            {
                dic.Add(DataNames.SceneLights, _extras.SceneLights.GetData());
            }
            if (_extras.TakedownLights != null)
            {
                dic.Add(DataNames.TakedownLights, _extras.TakedownLights.GetData());

            }
            if (_extras.Board.HasBoard)
            {
                dic.Add(DataNames.Board, _extras.Board.GetData());
            }
            dic.Add(DataNames.PrimaryPattern, CurrentPrmPattern);
            dic.Add(DataNames.SecondaryPattern, CurrentSecPattern);
            dic.Add(DataNames.WarningPattern, CurrentWrnPattern);
            dic.Add(DataNames.Stage, _stage.CurrentStage);
            if (SpotLight != null)
            {
                dic.Add(DataNames.Spotlight, SpotLight.GetData());
            }
            if (Scene != null)
            {
                dic.Add(DataNames.Scene, Scene.GetData());
            }
            return dic;
        }

        public Dictionary<int, object> GetPrimaryLightsData()
        {
            var prm = new Dictionary<int, object>();
            foreach (Extra.Extra e in _extras.PrimaryLights.Values)
            {
                prm.Add(e.Id, e.GetData());
            }

            return prm;
        }

        public Dictionary<int, object> GetSecondaryLightsData()
        {
            var sec = new Dictionary<int, object>();
            foreach (Extra.Extra e in _extras.SecondaryLights.Values)
            {
                sec.Add(e.Id, e.GetData());
            }

            return sec;
        }

        public Dictionary<int, object> GetWarningLightsData()
        {
            var wrn = new Dictionary<int, object>();
            foreach (Extra.Extra e in _extras.WarningLights.Values)
            {
                wrn.Add(e.Id, e.GetData());
            }
            return wrn;
        }

        public void SetData(IDictionary<string, object> data)
        {
            if (data.TryGetValue(DataNames.PRML, out var prml))
            {
                IDictionary<string, object> prm = (IDictionary<string, object>)prml;
                foreach (Extra.Extra e in _extras.PrimaryLights.Values)
                {
                    e.SetData((IDictionary<string, object>)prm[$"{e.Id}"]);
                }
            }
            if (data.TryGetValue(DataNames.SECL, out var secl))
            {
                IDictionary<string, object> sec = (IDictionary<string, object>)secl;
                foreach (Extra.Extra e in _extras.SecondaryLights.Values)
                {
                    e.SetData((IDictionary<string, object>)sec[$"{e.Id}"]);
                }
            }
            if (data.TryGetValue(DataNames.WRNL, out var wrnl))
            {
                IDictionary<string, object> wrn = (IDictionary<string, object>)wrnl;
                foreach (Extra.Extra e in _extras.WarningLights.Values)
                {
                    e.SetData((IDictionary<string, object>)wrn[$"{e.Id}"]);
                }
            }
            try
            {
                if (data.TryGetValue(DataNames.SteadyBurn, out var sbrn))
                {
                    _extras.SteadyBurn.SetData((IDictionary<string, object>)sbrn);
                }
            }
            catch (Exception e)
            {
                Utils.DebugWriteLine($"SBRN error: {e.Message}");
            }
            try
            {
                if (data.TryGetValue(DataNames.SceneLights, out var scl))
                {
                    _extras.SceneLights.SetData((IDictionary<string, object>)scl);
                }
            }
            catch (Exception e)
            {
                Utils.DebugWriteLine($"SCL error: {e.Message}");
            }
            try
            {
                if (data.TryGetValue(DataNames.TakedownLights, out var tdl))
                {
                    _extras.TakedownLights.SetData((IDictionary<string, object>)tdl);
                }
            }
            catch (Exception e)
            {
                Utils.DebugWriteLine($"TDL error: {e.Message}");
            }
            try
            {
                if (data.TryGetValue(DataNames.Board, out var brd))
                {
                    _extras.Board.SetData((IDictionary<string, object>)brd);
                }
            }
            catch (Exception e)
            {
                Utils.DebugWriteLine($"BRD error: {e.Message}");
            }
            if (data.TryGetValue(DataNames.PrimaryPattern, out var prmPatt))
            {
                CurrentPrmPattern = (int)prmPatt;
            }
            if (data.TryGetValue(DataNames.SecondaryPattern, out var secPatt))
            {
                CurrentSecPattern = int.Parse(secPatt.ToString());
            }
            if (data.TryGetValue(DataNames.WarningPattern, out var wrnPatt))
            {
                CurrentWrnPattern = int.Parse(wrnPatt.ToString());
            }
            if (data.TryGetValue(DataNames.Stage, out var stage))
            {
                _stage.SetStage(int.Parse(stage.ToString()));
            }
            if (SpotLight != null)
            {
                SpotLight.SetData((IDictionary<string, object>)data[DataNames.Spotlight]);
            }
            if (Scene != null)
            {
                Scene.SetData((IDictionary<string, object>)data[DataNames.Scene]);
            }
        }
    }
}
