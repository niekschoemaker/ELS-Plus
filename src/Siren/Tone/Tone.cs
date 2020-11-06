using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace ELS.Siren
{
    public enum ToneType
    {
        Horn,
        SrnTon1,
        SrnTon2,
        SrnTon3,
        SrnTon4,
        SrnPnic
    }
    class Tone
    {

        private readonly string _file;
        private readonly string _soundSet;
        private readonly ELSVehicle _entity;
        private int soundId = -2;
        internal string Type;

        internal bool State { private set; get; }
        internal bool AllowUse { get; set; }
        internal int LastVehicle { get; set; }

        internal Tone(string file, ELSVehicle entity, ToneType type, bool allow, bool state = false, string soundSet = null)
        {
            _entity = entity;
            _file = file;
            _soundSet = soundSet;
            Utils.ReleaseWriteLine($"siren: {type}; soundSet: {soundSet}");
            SetState(state);
            AllowUse = allow;
            switch (type)
            {
                case ToneType.SrnTon1:
                    Type = "WL";
                    break;
                case ToneType.SrnTon2:
                    Type = "YP";
                    break;
                case ToneType.SrnTon3:
                    Type = "A1";
                    break;
                case ToneType.SrnTon4:
                    Type = "A2";
                    break;
            }
            soundId = -2;
        }

        internal void RunTick()
        {
            var vehicle = _entity.Vehicle;
            if (!Vehicle.Exists(vehicle))
            {
                if (soundId != -2)
                {
                    CleanUp();
                }
            }
            if (State && AllowUse && soundId == -2)
            {
                soundId = API.GetSoundId();
                API.PlaySoundFromEntity(soundId, _file, vehicle.Handle, _soundSet, false, 0);
            }
        }

        internal void SetState(bool state)
        {
            var vehicle = _entity.Vehicle;
            State = state;
            if (vehicle == null)
            {
                return;
            }

            if (State && AllowUse)
            {
                if (soundId != -2)
                {
                    Audio.StopSound(soundId);
                    Audio.ReleaseSound(soundId);
                    soundId = -2;
                }
                if (soundId == -2)
                {
                    soundId = API.GetSoundId();
                    Utils.ReleaseWriteLine($"_file: {_file}; soundSet: {_soundSet}");
                    API.PlaySoundFromEntity(soundId, _file, vehicle.Handle, _soundSet, false, 0);
                }
            }
            else
            {
                if (soundId != -2)
                {
                    Audio.StopSound(soundId);
                    Audio.ReleaseSound(soundId);
                }
                if (Audio.HasSoundFinished(soundId)) {
                    soundId = -2;
                }
                else
                {
                    if(soundId != -2)
                    {
                        var _soundId = soundId;
                        BaseScript.Delay(100).ContinueWith(p =>
                        {
                            Audio.StopSound(_soundId);
                            Audio.ReleaseSound(_soundId);
                            if (Audio.HasSoundFinished(_soundId) && soundId == _soundId)
                            {
                                soundId = -2;
                            }
                        });
                    }
                }
            }
        }

        internal void CleanUp()
        {
            if(soundId == -2)
            {
                return;
            }
            Audio.StopSound(soundId);
            Audio.ReleaseSound(soundId);
            soundId = -2;
        }
    }
}
