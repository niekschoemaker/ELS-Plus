namespace ELS.Siren
{
    internal interface IManagerEntry 
    {
        void CleanUP(bool tooFarAwayCleanup = false);
        void ControlTicker(Light.Lights lights);
    }
}