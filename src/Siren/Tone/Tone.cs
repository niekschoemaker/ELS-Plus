using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private int soundId;
        private Entity _entity;
        private readonly ToneType _type;
        internal string Type;
        internal bool _state { private set; get; }
        internal bool AllowUse { get; set; }
        internal Tone(string file, Entity entity, ToneType type, bool allow, bool state = false)
        {
            _entity = entity;
            _file = file;
            _type = type;
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
            soundId = -1;
        }

        internal void SetState(bool state)
        {
            _state = state;
            if (_state && AllowUse)
            {
                if (soundId != -1)
                {
                    Audio.StopSound(soundId);
                    Audio.ReleaseSound(soundId);
                    if (Audio.HasSoundFinished(soundId))
                    {
                        soundId = -1;
                    }
                }
                if (soundId == -1)
                {
                    soundId = API.GetSoundId();
                    //Utils.ReleaseWriteLine($"Audio with id of {soundId}, hasFinished: {Audio.HasSoundFinished(soundId)}");
                    if (!Audio.HasSoundFinished(soundId)) return;
                    //soundId = Audio.PlaySoundFromEntity(_entity, _file);
                    API.PlaySoundFromEntity(soundId, _file, _entity.Handle, null, false, 0);
                    //Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, soundId, (InputArgument)_file, (InputArgument)_entity.Handle, (InputArgument)0, (InputArgument)0, (InputArgument)0);
                    //Utils.ReleaseWriteLine($"Started sound with id of {soundId}");
                }
            }
            else
            {
                Audio.StopSound(soundId);
                Audio.ReleaseSound(soundId);
                if (Audio.HasSoundFinished(soundId)) {
                    Utils.ReleaseWriteLine($"Stopped and released sound with id of {soundId}");
                    soundId = -1;
                }
                else
                {
                    Utils.ReleaseWriteLine("Siren still playing?");
                    var _soundId = soundId;
                    Task.Delay(100).ContinueWith(p =>
                    {
                        Audio.StopSound(_soundId);
                        Audio.ReleaseSound(_soundId);
                        if (Audio.HasSoundFinished(_soundId) && soundId == _soundId)
                        {
                            Utils.ReleaseWriteLine($"Stopped and released sound with id of {soundId}");
                            soundId = -1;
                        }
                    });
                }
            }
        }

        internal void CleanUp()
        {
#if DEBUG
            CitizenFX.Core.Debug.WriteLine("Tone deconstructor ran");
#endif
            Audio.StopSound(soundId);
            Audio.ReleaseSound(soundId);
        }
    }
}
