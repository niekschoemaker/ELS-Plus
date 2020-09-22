using CitizenFX.Core;
using ELS.Light;

namespace ELS.Siren
{
    partial class Siren : IManagerEntry
    {
        public void Ticker()
        {
            _tones.RunTick();
        }

        public void ControlTicker(Lights lights)
        {
            Game.DisableControlThisFrame(0, configuration.ElsConfiguration.KBBindings.Sound_Ahorn);
            AirHornControlsKB();
            ManualTone1ControlsKB();
            ManualTone2ControlsKB();
            ManualTone3ControlsKB();
            ManualTone4ControlsKB();
            ManualSoundControlsKB();
            if (lights._stage.CurrentStage == 3)
            {
                MainSirenToggleControlsKB();
            }
            DualSirenControlsKB();
        }
    }
}
