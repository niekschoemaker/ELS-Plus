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
                AirHornLogic(true, true);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.AirHorn, _elsVehicle, true);

            }
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Sound_Ahorn) &&
                 Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                || (Game.IsControlJustReleased(2, Control.SpecialAbility) && Game.CurrentInputMode == InputMode.GamePad && Global.AllowController))
            {
                AirHornLogic(false, true);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.AirHorn, _elsVehicle, false);
            }
        }
        void ManualTone0ControlsKB()
        {
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Snd_SrnTon1) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) || (Global.AllowController && Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.Snd_SrnTon1) && Game.CurrentInputMode == InputMode.GamePad))
            {
                if (SirenTone0Logic(true, true)) {
                    RemoteEventManager.SendSirenEvent(_elsVehicle, "ms", new { c = 0 });
                }
            }
        }
        void ManualTone1ControlsKB()
        {
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Snd_SrnTon2) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) || (Global.AllowController && Game.IsControlPressed(2, ElsConfiguration.GPBindings.Snd_SrnTon1) && Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.Snd_SrnTon3) && Game.CurrentInputMode == InputMode.GamePad))
            {
                if (SirenTone1Logic(true, true))
                {
                    RemoteEventManager.SendSirenEvent(_elsVehicle, "ms", new { c = 1 });
                }
            }
        }
        void ManualTone2ControlsKB()
        {
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Snd_SrnTon3) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) || (Global.AllowController && Game.IsControlJustPressed(2, ElsConfiguration.GPBindings.Snd_SrnTon3) && Game.CurrentInputMode == InputMode.GamePad))
            {
                if (SirenTone2Logic(true, true))
                {
                    RemoteEventManager.SendSirenEvent(_elsVehicle, "ms", new { c = 2 });
                }
            }
        }
        void ManualTone3ControlsKB()
        {
            if (Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Snd_SrnTon4) && SirenTone3Logic(true, true))
            {
                RemoteEventManager.SendSirenEvent(_elsVehicle, "ms", new { c = 3 });
            }
        }

        void MainSirenToggleControlsKB()
        {
            if ((Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Toggle_SIRN) 
                && Game.CurrentInputMode == InputMode.MouseAndKeyboard) || (Global.AllowController 
                && Game.IsControlJustReleased(2, ElsConfiguration.GPBindings.Toggle_SIRN) 
                && Game.CurrentInputMode == InputMode.GamePad))
            {
                MainSirenToggleLogic(true, true);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.MainSiren, _elsVehicle, true);
            }
        }

        void ManualSoundControlsKB()
        {
            Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Sound_Manul);
            if (Game.IsDisabledControlJustPressed(0, ElsConfiguration.KBBindings.Sound_Manul))
            {
                ManualSoundLogic(true, true);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ManualSound, _elsVehicle, true);
            }
            if (Game.IsDisabledControlJustReleased(0, ElsConfiguration.KBBindings.Sound_Manul))
            {
                ManualSoundLogic(false, true);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.ManualSound, _elsVehicle, false);
            }
        }

        void DualSirenControlsKB()
        {
            if (Game.IsControlJustReleased(0, ElsConfiguration.KBBindings.Toggle_DSRN))
            {
                DualSirenLogic(true, true);
                RemoteEventManager.SendEvent(RemoteEventManager.Commands.DualSiren, _elsVehicle, true);
            }
        }
    }
}