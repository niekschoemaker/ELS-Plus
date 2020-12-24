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
using CitizenFX.Core;
using CitizenFX.Core.Native;
using ELS.configuration;
using ELS.NUI;
using System;
using System.Collections.Generic;

namespace ELS.Light
{
    enum ExtraEnum
    {
        PRML,
        WRNL,
        SECL
    }
    internal struct Extras
    {
        public Dictionary<int, Extra.Extra> PrimaryLights;
        internal Dictionary<int, Extra.Extra> WarningLights;
        internal Dictionary<int, Extra.Extra> SecondaryLights;
        internal Extra.Extra SteadyBurn;
        internal Extra.Extra SceneLights;
        internal Extra.Extra TakedownLights;
        internal Board.ArrowBoard Board;
        internal Gadgets.Ladder Ladder;
    }

    partial class Lights : IManagerEntry, ILight
    {
        private Extras _extras = new Extras
        {
            PrimaryLights = new Dictionary<int, Extra.Extra>(),
            WarningLights = new Dictionary<int, Extra.Extra>(),
            SecondaryLights = new Dictionary<int, Extra.Extra>(),
        };

        public Vcfroot Vcfroot { get; set; }
        public Vehicle Vehicle { get; set; }
        public ELSVehicle ElsVehicle { get; set; }
        internal Stage _stage;
        public Scene Scene { get; set; }
        public SpotLight SpotLight { get; set; }
        private bool _isInitialized = false;
       

        internal Lights(ELSVehicle vehicle, Vcfroot vcfroot)
        {
            Vcfroot = vcfroot;
            ElsVehicle = vehicle;
            Vehicle = vehicle.Vehicle;
            LightStagesSetup();
            if (Vehicle.Exists(Vehicle))
            {
                Init();
            }
        }

        private void LightStagesSetup()
        {
            if (Vcfroot == null)
            {
                if (Utils.IsDeveloper)
                {
                    Utils.DebugWriteLine($"_vcfroot: {Vcfroot == null} networkId: {ElsVehicle?.NetworkId}; INTERFACE: {Vcfroot?.INTERFACE?.LstgActivationType == null}");
                }
                return;
            }
            _stage = new Stage(Vcfroot.PRML, Vcfroot.SECL, Vcfroot.WRNL, ElsVehicle.NetworkId, Vcfroot.INTERFACE?.LstgActivationType);
        }

        private void Init()
        {
            Vehicle = ElsVehicle.Vehicle;
            if (!Vehicle.Exists(Vehicle))
            {
                Vehicle = null;
                return;
            }
            if (_isInitialized)
            {
                return;
            }
            AddAllValidLightExtras();
            SetupPatternsPrm();
            SetupSecPatterns();
            SetupWrnPatterns();
            _isInitialized = true;
        }

        internal void SyncUi()
        {
            ElsUiPanel.ToggleStages(_stage.CurrentStage);
            if (Vcfroot.INTERFACE.LstgActivationType.ToLower().Equals("euro"))
            {
                ElsUiPanel.SetEuro(true);
            }
            else
            {
                ElsUiPanel.SetEuro(false);
            }
            ElsUiPanel.SetUiDesc(_prefix + CurrentPrmPattern.ToString().PadLeft(3, '0'), ExtraEnum.PRML.ToString());
            ElsUiPanel.SetUiDesc(_prefix + CurrentSecPattern.ToString().PadLeft(3, '0'), ExtraEnum.SECL.ToString());
            ElsUiPanel.SetUiDesc(_prefix + CurrentWrnPattern.ToString().PadLeft(3, '0'), ExtraEnum.WRNL.ToString());
            ElsUiPanel.ToggleUiBtnState(prmLights, "PRML");
            ElsUiPanel.ToggleUiBtnState(secLights, "SECL");
            ElsUiPanel.ToggleUiBtnState(wrnLights, "WRNL");
            ElsUiPanel.ToggleUiBtnState(crsLights, "CRS");
            if (Scene != null)
            {
                ElsUiPanel.ToggleUiBtnState(Scene.TurnedOn, "SCL");
            }
            if (SpotLight != null)
            {
                ElsUiPanel.ToggleUiBtnState(SpotLight.TurnedOn, "TDL");
            }
        }

        internal void SetGTASirens(bool state) {
            ElsVehicle.IsSirenActive = state;
            var distance = Game.Player.Character.Position.DistanceToSquared(Vehicle.Position);
            if (distance < (ELSVehicle.deactivateDistance * ELSVehicle.deactivateDistance) || state == false)
            {
#if SIREN
                _vehicle.IsSirenActive = state;
#endif
            }
            else
            {
#if SIREN
                _vehicle.IsSirenActive = false;
#endif
            }
        }

        private void AddAllValidLightExtras()
        {
            for (int x = 1; x < 13; x++)
            {
                switch (x)
                {
                    case 1:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 1) && Vcfroot.EOVERRIDE.Extra01.IsElsControlled)
                            {
                                _extras.PrimaryLights.Add(1, new Extra.Extra(this, 1, Vcfroot.EOVERRIDE.Extra01, Vcfroot.PRML.LightingFormat));
                            }
                        }
                        break;
                    case 2:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 2) && Vcfroot.EOVERRIDE.Extra02.IsElsControlled)
                            {
                                _extras.PrimaryLights.Add(2, new Extra.Extra(this, 2, Vcfroot.EOVERRIDE.Extra02, Vcfroot.PRML.LightingFormat));
                            }
                        }
                        break;
                    case 3:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 3) && Vcfroot.EOVERRIDE.Extra03.IsElsControlled)
                            {
                                _extras.PrimaryLights.Add(3, new Extra.Extra(this, 3, Vcfroot.EOVERRIDE.Extra03, Vcfroot.PRML.LightingFormat));
                            }
                        }
                        break;
                    case 4:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 4) && Vcfroot.EOVERRIDE.Extra04.IsElsControlled)
                            {
                                _extras.PrimaryLights.Add(4, new Extra.Extra(this, 4, Vcfroot.EOVERRIDE.Extra04, Vcfroot.PRML.LightingFormat));
                            }
                        }
                        break;
                    case 5:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 5) && Vcfroot.EOVERRIDE.Extra05.IsElsControlled)
                            {
                                _extras.WarningLights.Add(5, new Extra.Extra(this, 5, Vcfroot.EOVERRIDE.Extra05, Vcfroot.WRNL.LightingFormat));
                            }
                        }
                        break;
                    case 6:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 6) && Vcfroot.EOVERRIDE.Extra06.IsElsControlled)
                            {
                                _extras.WarningLights.Add(6, new Extra.Extra(this, 6, Vcfroot.EOVERRIDE.Extra06, Vcfroot.WRNL.LightingFormat));
                            }
                        }
                        break;
                    case 7:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 7) && Vcfroot.EOVERRIDE.Extra07.IsElsControlled)
                            {
                                _extras.SecondaryLights.Add(7, new Extra.Extra(this, 7, Vcfroot.EOVERRIDE.Extra07, Vcfroot.SECL.LightingFormat));
                            }
                        }
                        break;
                    case 8:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 8) && Vcfroot.EOVERRIDE.Extra08.IsElsControlled)
                            {
                                _extras.SecondaryLights.Add(8, new Extra.Extra(this, 8, Vcfroot.EOVERRIDE.Extra08, Vcfroot.SECL.LightingFormat));
                            }
                        }
                        break;
                    case 9:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 9) && Vcfroot.EOVERRIDE.Extra09.IsElsControlled)
                            {
                                _extras.SecondaryLights.Add(9, new Extra.Extra(this, 9, Vcfroot.EOVERRIDE.Extra09, Vcfroot.SECL.LightingFormat));
                            }
                        }
                        break;
                    case 10:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 10) && Vcfroot.EOVERRIDE.Extra10.IsElsControlled)
                            {
                                _extras.SteadyBurn = new Extra.Extra(this, 10, Vcfroot.EOVERRIDE.Extra10);
                            }
                        }
                        break;
                    case 11:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 11) && Vcfroot.EOVERRIDE.Extra11.IsElsControlled && Vcfroot.MISC.Takedowns.AllowUse)
                            {
                                _extras.TakedownLights = new Extra.Extra(this, 11, Vcfroot.EOVERRIDE.Extra11);
                            }
                            else if (Vcfroot.MISC.Takedowns.AllowUse)
                            {
                                SpotLight = new SpotLight(this);
                            }
                        }
                        break;
                    case 12:
                        {
                            if (API.DoesExtraExist(Vehicle.Handle, 12) && Vcfroot.EOVERRIDE.Extra12.IsElsControlled && Vcfroot.MISC.SceneLights.AllowUse)
                            {
                                _extras.SceneLights = new Extra.Extra(this, 12, Vcfroot.EOVERRIDE.Extra12);
                            }
                            else if (Vcfroot.MISC.SceneLights.AllowUse)
                            {
                                Scene = new Scene(this);
                            }
                        }
                        break;
                }
            }
            if (!String.IsNullOrEmpty(Vcfroot.MISC.ArrowboardType))
            {
                switch (Vcfroot.MISC.ArrowboardType)
                {
                    case "bonnet":
                        _extras.Board = new Board.ArrowBoard(this, Vcfroot.MISC);
                        break;
                    case "boot":
                        _extras.Board = new Board.ArrowBoard(this, Vcfroot.MISC);
                        break;
                    case "boot2":
                        _extras.Board = new Board.ArrowBoard(this, Vcfroot.MISC);
                        break;
                    case "boots":
                        _extras.Board = new Board.ArrowBoard(this, Vcfroot.MISC);
                        break;
                    case "off":
                        _extras.Board = new Board.ArrowBoard(this, Vcfroot.MISC);
                        break;
                    default:
                        _extras.Board = new Board.ArrowBoard(this, Vcfroot.MISC);
                        break;
                }
            }
            if (Vcfroot.MISC.HasLadderControl)
            {
                _extras.Ladder = new Gadgets.Ladder(this, Vcfroot.MISC);
            }
            if (_extras.PrimaryLights.Count == 0)
            {
                _prmPatterns = 0;
                ElsUiPanel.SetUiDesc("--", ExtraEnum.PRML.ToString());
            }
            if (_extras.SecondaryLights.Count == 0)
            {
                _secPatterns = 0;
                ElsUiPanel.SetUiDesc("--", ExtraEnum.SECL.ToString());
            }
            if (_extras.WarningLights.Count == 0)
            {
                _wrnPatterns = 0;
                ElsUiPanel.SetUiDesc("--", ExtraEnum.WRNL.ToString());
            }
        }


        Vehicle IManagerEntry._vehicle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void CleanUP(bool tooFarAwayCleanup = false)
        {
            foreach(Extra.Extra e in _extras.PrimaryLights.Values)
            {
                e.CleanUp();
            }
            foreach (Extra.Extra e in _extras.SecondaryLights.Values)
            { 
                e.CleanUp();
            }
            foreach (Extra.Extra e in _extras.WarningLights.Values)
            {
                e.CleanUp();
            }
            if (_extras.SteadyBurn != null)
            {
                _extras.SteadyBurn.CleanUp();
            }
            if (_extras.TakedownLights != null)
            {
               _extras.TakedownLights.CleanUp();
            }
            if (_extras.SceneLights != null)
            {
               _extras.SceneLights.CleanUp();
            }
            Vehicle = null;
        }
    }
}
