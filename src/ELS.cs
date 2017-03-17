﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using ELS.Siren;
using Control = CitizenFX.Core.Control;
using CitizenFX.Core.Native;
using System;

namespace ELS
{
    public class ELS : BaseScript
    {
        public static bool isStopped;
        private SirenManager _sirenManager;
        private FileLoader _FileLoader;
        private configuration.ControlConfiguration controlConfiguration;
        public ELS()
        {
            controlConfiguration = new configuration.ControlConfiguration();
            _FileLoader = new FileLoader(this);
            _sirenManager = new SirenManager();
            EventHandlers["onClientResourceStart"] += new Action<string>(_FileLoader.RunLoadeer);
            EventHandlers["ELS:SirenUpdated"] += new Action<int>(_sirenManager.UpdateSirens);
            EventHandlers["onClientResourceStop"] += new Action(() => {
                isStopped = true;
            });
            Tick += Class1_Tick;
        }

        private async Task Class1_Tick()
        {

            if (LocalPlayer.Character.IsGettingIntoAVehicle)
            {
                if (LocalPlayer.Character.VehicleTryingToEnter != null)
                {
                    _sirenManager.SetCurrentSiren(LocalPlayer.Character.VehicleTryingToEnter);
                }
            }
            if (LocalPlayer.Character.IsInVehicle())
            {
                Screen.ShowNotification(Function.Call<int>(Hash.NETWORK_GET_NETWORK_ID_FROM_ENTITY, LocalPlayer.Character.CurrentVehicle.Handle).ToString());
                _sirenManager.runtick();
            }
        }
    }
}

