using CitizenFX.Core;
using CitizenFX.Core.Native;
using ELS.configuration;
using ELS.NUI;
using System.Collections.Generic;

namespace ELS.Light
{
    internal enum IndicatorState
    {
        Off,
        Left,
        Right,
        Hazard
    }

    internal class Indicator : BaseScript
    {

        public static IndicatorState CurrentIndicatorState(Vehicle veh) {
            return (IndicatorState)API.GetVehicleIndicatorLights(veh.Handle);
        }

        public static Dictionary<string, IndicatorState> IndStateLib = new Dictionary<string, IndicatorState>()
        {
            { "Off", IndicatorState.Off },
            { "Left", IndicatorState.Left },
            { "Right", IndicatorState.Right },
            { "Hazard", IndicatorState.Hazard }
        };

        public static int IndicatorDelay { get; set; }

        public static bool IsHazardLightActive { get; set; }

        public static void ToggleInicatorState(Vehicle veh, IndicatorState state)
        {
            switch (state)
            {
                case IndicatorState.Off:
                    veh.IsLeftIndicatorLightOn = false;
                    veh.IsRightIndicatorLightOn = false;
                    break;
                case IndicatorState.Left:
                    veh.IsLeftIndicatorLightOn = true;
                    veh.IsRightIndicatorLightOn = false;
                    break;
                case IndicatorState.Right:
                    veh.IsLeftIndicatorLightOn = false;
                    veh.IsRightIndicatorLightOn = true;
                    break;
                case IndicatorState.Hazard:
                    veh.IsLeftIndicatorLightOn = true;
                    veh.IsRightIndicatorLightOn = true;
                    break;
            }
        }

        public static void ToggleRightBlinker()
        {
            var veh = ELS.CurrentVehicle;
            if (CurrentIndicatorState(veh) == IndicatorState.Right)
            {
                ToggleInicatorState(veh, IndicatorState.Off);
                if (Global.BtnClicksIndicators)
                {
                    ElsUiPanel.PlayUiSound("indoff");
                }
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ToggleInd, veh, true);
                return;
            }
            ToggleInicatorState(veh, IndicatorState.Right);
            if (Global.BtnClicksIndicators)
            {
                ElsUiPanel.PlayUiSound("indon");
            }
            RemoteEventManager.SendEvent(RemoteEventManager.Commands.ToggleInd, veh, true);
        }

        [Command("togglerightblinker")]
        public static void ToggleRightBlinkerCommand()
        {
            if (ELS.CurrentVehicle != null && Vehicle.Exists(ELS.CurrentVehicle) && Game.IsControlEnabled(0, ElsConfiguration.KBBindings.ToggleRIND))
            {
                ToggleRightBlinker();
            }
        }

        public static void ToggleLeftBlinker()
        {
            var veh = ELS.CurrentVehicle;
            if (CurrentIndicatorState(veh) == IndicatorState.Left)
            {
                ToggleInicatorState(veh, IndicatorState.Off);
                if (Global.BtnClicksIndicators)
                {
                    ElsUiPanel.PlayUiSound("indoff");
                }
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ToggleInd, veh, true);
                return;
            }

            ToggleInicatorState(veh, IndicatorState.Left);
            if (Global.BtnClicksIndicators)
            {
                ElsUiPanel.PlayUiSound("indon");
            }
            RemoteEventManager.SendEvent(RemoteEventManager.Commands.ToggleInd, veh, true);
        }

        [Command("toggleleftblinker")]
        public static void ToggleLeftBlinkerCommand()
        {
            if (ELS.CurrentVehicle != null && Vehicle.Exists(ELS.CurrentVehicle) && Game.IsControlEnabled(0, ElsConfiguration.KBBindings.ToggleLIND))
            {
                ToggleLeftBlinker();
            }
        }

        public static void SetHazards(bool turnOn)
        {
            var veh = ELS.CurrentVehicle;
            if (!turnOn)
            {
                ToggleInicatorState(veh, IndicatorState.Off);
                IsHazardLightActive = false;
                if (Global.BtnClicksIndicators)
                {
                    ElsUiPanel.PlayUiSound("indoff");
                }
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ToggleInd, veh, true);
                return;
            }

            ToggleInicatorState(veh, IndicatorState.Hazard);
            IsHazardLightActive = true;
            if (Global.BtnClicksIndicators)
            {
                ElsUiPanel.PlayUiSound("indon");
            }
            RemoteEventManager.SendEvent(RemoteEventManager.Commands.ToggleInd, veh, true);
        }

        public static void ToggleHazards()
        {
            var veh = ELS.CurrentVehicle;
            if (CurrentIndicatorState(veh) == IndicatorState.Hazard)
            {
                SetHazards(false);
            }
            else
            {
                SetHazards(true);
            }
        }

        [Command("togglehazard")]
        public static void ToggleHazardsCommand()
        {
            if (ELS.CurrentVehicle != null && Vehicle.Exists(ELS.CurrentVehicle) && !Game.IsPaused && Game.IsControlEnabled(0, ElsConfiguration.KBBindings.ToggleHAZ))
            {
                ToggleHazards();
            }
        }
    }
}
