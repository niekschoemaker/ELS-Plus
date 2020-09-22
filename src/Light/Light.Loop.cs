using CitizenFX.Core;
using CitizenFX.Core.Native;
using ELS.configuration;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELS.Light
{
    partial class Lights
    {

        public async void ControlTicker()
        {
            if (!Vehicle.Exists(Vehicle))
            {
                Init();
                if (!Vehicle.Exists(Vehicle))
                {
                    return;
                }
            }
            //KB Controls
            ToggleSecLKB();
            ToggleWrnLKB();
            ToggleBrdKB();
            ToggleCrsKB();
            ChgPrmPattKB();
            ChgWrnPattKB();
            ChgSecPattKB();
            ToggleTdlKB();
            ToggleSclKB();
            ToggleLightStageKB();
        }



        public async void Ticker()
        {
            if (!Vehicle.Exists(Vehicle))
            {
                Init();
                if (!Vehicle.Exists(Vehicle))
                {
                    return;
                }
            }
            if (_extras.SteadyBurn != null)
            {
                _extras.SteadyBurn.ExtraTicker();
            }
            if (_extras.SceneLights != null)
            {
                _extras.SceneLights.ExtraTicker();
            }
            if (_extras.TakedownLights != null)
            {
                _extras.TakedownLights.ExtraTicker();
            }

            if (SpotLight != null && SpotLight.TurnedOn)
            {
                SpotLight.RunTick();
            }
            if (Scene != null && Scene.TurnedOn)
            {
                Scene.RunTick();
            }
            //foreach (Extra.Extra prim in _extras.PRML.Values)
            for (int i = 0; i < _extras.PrimaryLights.Count; i++)
            {
                _extras.PrimaryLights.ElementAt(i).Value.ExtraTicker();
                if (_stage != null)
                {
                    switch (_stage.CurrentStage)
                    {
                        case 1:
                            if (!String.IsNullOrEmpty(_stage.PRML.PresetPatterns.Lstg1.Pattern) && _stage.PRML.PresetPatterns.Lstg1.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                        case 2:
                            if (!String.IsNullOrEmpty(_stage.PRML.PresetPatterns.Lstg1.Pattern) && _stage.PRML.PresetPatterns.Lstg2.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                        case 3:
                            if (!String.IsNullOrEmpty(_stage.PRML.PresetPatterns.Lstg1.Pattern) && _stage.PRML.PresetPatterns.Lstg3.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                    }
                }
            }
            //foreach (Extra.Extra sec in _extras.SECL.Values)
            for (int i = 0; i < _extras.SecondaryLights.Count; i++)
            {
                _extras.SecondaryLights.ElementAt(i).Value.ExtraTicker();
                if (_stage != null)
                {
                    switch (_stage.CurrentStage)
                    {
                        case 1:
                            if (!String.IsNullOrEmpty(_stage.SECL.PresetPatterns.Lstg1.Pattern) && _stage.SECL.PresetPatterns.Lstg1.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                        case 2:
                            if (!String.IsNullOrEmpty(_stage.SECL.PresetPatterns.Lstg1.Pattern) && _stage.SECL.PresetPatterns.Lstg2.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                        case 3:
                            if (!String.IsNullOrEmpty(_stage.SECL.PresetPatterns.Lstg1.Pattern) && _stage.SECL.PresetPatterns.Lstg3.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                    }
                }
            }
            //foreach (Extra.Extra wrn in _extras.WRNL.Values)
            for (int i = 0; i < _extras.WarningLights.Count; i++)
            {
                _extras.WarningLights.ElementAt(i).Value.ExtraTicker();
                if (_stage != null)
                {
                    switch (_stage.CurrentStage)
                    {
                        case 1:
                            if (!String.IsNullOrEmpty(_stage.WRNL.PresetPatterns.Lstg1.Pattern) && _stage.WRNL.PresetPatterns.Lstg1.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                        case 2:
                            if (!String.IsNullOrEmpty(_stage.WRNL.PresetPatterns.Lstg1.Pattern) && _stage.WRNL.PresetPatterns.Lstg2.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                        case 3:
                            if (!String.IsNullOrEmpty(_stage.WRNL.PresetPatterns.Lstg1.Pattern) && _stage.WRNL.PresetPatterns.Lstg3.Pattern.ToLower().Equals("scan") && _scan)
                            {
                                ScanPatternTicker();
                            }
                            break;
                    }
                }
            }
        }

        int _patternStart = 0;
        internal void ScanPatternTicker()
        {
            if (Game.GameTime - _patternStart > 15000)
            {
                _patternStart = Game.GameTime;
#if DEBUG
                CitizenFX.Core.Debug.WriteLine("Toggling scan pattern");
#endif
                ToggleScanPattern();
            }
            else if (_patternStart == 0)
            {
                _patternStart = Game.GameTime;
            }
        }

        internal void ToggleSecLKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ToggleSecL);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ToggleSecL) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ToggleSecLights();
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.SECL, GetSecondaryLightsData());
            }
        }

        internal void ToggleWrnLKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ToggleWrnL);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ToggleWrnL) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ToggleWrnLights();
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.WRNL, GetWarningLightsData());
            }
        }

        internal void ToggleBrdKB()
        {
            //Game.DisableControlThisFrame(0, Control.CharacterWheel);
            if (Game.IsControlJustPressed(0, ElsConfiguration.KBBindings.ToggleBoard) && Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
#if DEBUG
                CitizenFX.Core.Debug.WriteLine($"Is Board raised  {_extras.BRD.BoardRaised}");
#endif
                if (_extras.Board.BoardRaised)
                {
                    _extras.Board.RaiseBoardNow = false;
                }
                else
                {
                    _extras.Board.RaiseBoardNow = true;
                }
                //RemoteEventManager.SendEvent(RemoteEventManager.Commands.ToggleWrnL, _vehicle, true, Game.Player.ServerId);
            }
        }

        internal void ToggleCrsKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ToggleCrsL);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ToggleCrsL) && !Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ToggleCrs();
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.PRML, GetPrimaryLightsData());
            }
        }

        internal void ToggleTdlKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ToggleTdl);
            if ((Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ToggleTdl) && !Game.IsControlPressed(0, Control.CharacterWheel) && !API.IsPauseMenuActive()) && Game.CurrentInputMode == InputMode.MouseAndKeyboard || (Global.AllowController && Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.ToggleTdl) && Game.CurrentInputMode == InputMode.GamePad))
            {
                ToggleTdl();
                var dic = new Dictionary<string, object>();
                if (_extras.TakedownLights != null)
                {
                    dic.Add(DataNames.TakedownLights, _extras.TakedownLights.GetData());
                }
                if (SpotLight != null)
                {
                    dic.Add(DataNames.Spotlight, SpotLight.GetData());
                }

                RemoteEventManager.SendLightEvent(ElsVehicle, dic);
            }
        }

        internal void ToggleSclKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ToggleTdl);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ToggleTdl) && Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ToggleScl();
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.SceneLights, _extras.SceneLights.GetData());
            }
        }

        internal void ChgPrmPattKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ChgPattPrmL);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ChgPattPrmL) && !Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ChgPrmPatt(false);
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.PrimaryPattern, CurrentPrmPattern);
            }
            else if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ChgPattPrmL) && Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ChgPrmPatt(true);
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.PrimaryPattern, CurrentPrmPattern);
            }
        }

        internal void ChgSecPattKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ChgPattSecL);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ChgPattSecL) && !Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ChgSecPatt(false);
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.SecondaryPattern, CurrentSecPattern);
            }
            else if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ChgPattSecL) && Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ChgSecPatt(true);
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.SecondaryPattern, CurrentSecPattern);
            }
        }

        internal void ChgWrnPattKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ChgPattWrnL);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ChgPattWrnL) && !Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ChgWrnPatt(false);
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.WarningPattern, CurrentWrnPattern);
            }
            else if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ChgPattWrnL) && Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ChgWrnPatt(true);
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.WarningPattern, CurrentWrnPattern);
            }
        }

        internal void ToggleLightStageKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.ToggleLstg);
            if ((Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ToggleLstg) && !Game.IsControlPressed(0, Control.CharacterWheel)) && Game.CurrentInputMode == InputMode.MouseAndKeyboard || (Global.AllowController && Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.ToggleLstg) && Game.CurrentInputMode == InputMode.GamePad))
            {
                ToggleLightStage();
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.Stage, CurrentStage);
            }
            else if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.ToggleLstg) && Game.IsControlPressed(0, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ToggleLightStageInverse();
                RemoteEventManager.SendLightEvent(ElsVehicle, DataNames.Stage, CurrentStage);
            }
        }

    }
}
