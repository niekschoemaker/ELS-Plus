using CitizenFX.Core;

namespace ELS.Light
{
    internal interface IManagerEntry
    {
        void CleanUP(bool tooFarAwayCleanup = false);
        void Ticker();
        void ControlTicker();
        void LightsControlsRemote();
        Vehicle _vehicle { get; set; }
    }
}
