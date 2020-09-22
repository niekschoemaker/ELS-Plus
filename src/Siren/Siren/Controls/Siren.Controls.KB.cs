using CitizenFX.Core;
using ELS.configuration;
using System;

namespace ELS.Siren
{
    partial class Siren : IManagerEntry
    {
        void AirHornControlsKB()
        {
            if ((Game.IsControlJustPressed(0, ElsConfiguration.KBBindings.Sound_Ahorn) &&
                 Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                (Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.Sound_Ahorn) && Game.CurrentInputMode == InputMode.GamePad && Global.AllowController))
            {
#if !REMOTETEST
                AirHornLogic(true, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.AirHorn, _elsVehicle, true);

            }
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Sound_Ahorn) &&
                 Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                || (Game.IsControlJustReleased(2, Control.SpecialAbility) && Game.CurrentInputMode == InputMode.GamePad && Global.AllowController))
            {
#if !REMOTETEST
                int[] patts = { _patternController.CurrentPrmPattern, _patternController.CurrentSecPattern, _patternController.CurrentWrnPattern };
                AirHornLogic(false, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.AirHorn, _elsVehicle, false);
            }
        }
        void ManualTone1ControlsKB()
        {
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Snd_SrnTon1) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) || (Global.AllowController && Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.Snd_SrnTon1) && Game.CurrentInputMode == InputMode.GamePad))
            {
#if !REMOTETEST
                SirenTone1Logic(true, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ManualTone1, _elsVehicle, true);
            }
        }
        void ManualTone2ControlsKB()
        {
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Snd_SrnTon2) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) || (Global.AllowController && Game.IsControlPressed(2, ElsConfiguration.GPBindings.Snd_SrnTon1) && Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.Snd_SrnTon3) && Game.CurrentInputMode == InputMode.GamePad))
            {
#if !REMOTETEST
                SirenTone2Logic(true, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ManualTone2, _elsVehicle, true);
            }
        }
        void ManualTone3ControlsKB()
        {
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Snd_SrnTon3) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) || (Global.AllowController && Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.Snd_SrnTon3) && Game.CurrentInputMode == InputMode.GamePad))
            {
#if !REMOTETEST
                SirenTone3Logic(true, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ManualTone3, _elsVehicle, true);
            }
        }
        void ManualTone4ControlsKB()
        {
            if (Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Snd_SrnTon4))
            {
#if !REMOTETEST
                SirenTone4Logic(true, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ManualTone4, _elsVehicle, true);
            }
        }

        void MainSirenToggleControlsKB()
        {
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Toggle_SIRN) 
                && Game.CurrentInputMode == InputMode.MouseAndKeyboard) || (Global.AllowController 
                && Game.IsControlJustReleased(2, ElsConfiguration.GPBindings.Toggle_SIRN) 
                && Game.CurrentInputMode == InputMode.GamePad))
            {
#if !REMOTETEST
                MainSirenToggleLogic(true, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MainSiren, _elsVehicle, true);
            }
        }

        void ManualSoundControlsKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Sound_Manul);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.Sound_Manul))
            {
#if !REMOTETEST
                ManualSoundLogic(true, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ManualSound, _elsVehicle, true);
            }
            if (Game.IsDisabledControlJustReleased(0, ElsConfiguration.KBBindings.Sound_Manul))
            {
#if !REMOTETEST
                ManualSoundLogic(false, true);
#endif
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ManualSound, _elsVehicle, false);
            }
        }

        void DualSirenControlsKB()
        {
            if (Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Toggle_DSRN))
            {
#if !REMOTETEST
                DualSirenLogic(true, true);
#endif
                System.Collections.Generic.Dictionary<String, object> dic = new System.Collections.Generic.Dictionary<string, object>();
                // Manager.VehicleManager.SyncRequestReply(_vehicle.NetworkId);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.DualSiren, _elsVehicle, true);
            }
        }
    }
}