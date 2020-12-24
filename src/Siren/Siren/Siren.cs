using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using CitizenFX.Core.UI;
using ELS.configuration;
using System.Collections;
using System.Collections.Generic;
using ELS.NUI;
using ELS.Light;

namespace ELS.Siren
{
    internal delegate void StateChangedHandler(Tone tone);

    /// <summary>
    /// Has diffrent tones
    /// </summary>
    partial class Siren : IManagerEntry
    {
        private Vcfroot _vcf;
        internal bool dual_siren;
        public ELSVehicle _elsVehicle { get; set; }
        public Vehicle _vehicle { get; set; }
        public MainSiren _mainSiren;
        private bool _isInitialized = false;
        IPatterns _patternController;
        internal Tones _tones;
        public Siren(ELSVehicle vehicle,Vcfroot vcfroot, [Optional]IDictionary<string,object> data, IPatterns patt)
        {
            _vcf = vcfroot;
            _elsVehicle = vehicle;
            _patternController = patt;
#if DEBUG
            Utils.DebugWriteLine(_vehicle.DisplayName);
#endif
            _tones = new Tones
            {
                horn = new Tone(_vcf.SOUNDS.MainHorn.AudioString, _elsVehicle, ToneType.Horn, true, soundSet: _vcf.SOUNDS.MainHorn.SoundSet),
                tone1 = new Tone(_vcf.SOUNDS.SrnTone1.AudioString, _elsVehicle, ToneType.SrnTon1, _vcf.SOUNDS.SrnTone1.AllowUse, soundSet: _vcf.SOUNDS.SrnTone1.SoundSet),
                tone2 = new Tone(_vcf.SOUNDS.SrnTone2.AudioString, _elsVehicle, ToneType.SrnTon2, _vcf.SOUNDS.SrnTone2.AllowUse, soundSet: _vcf.SOUNDS.SrnTone2.SoundSet),
                tone3 = new Tone(_vcf.SOUNDS.SrnTone3.AudioString, _elsVehicle, ToneType.SrnTon3, _vcf.SOUNDS.SrnTone3.AllowUse, soundSet: _vcf.SOUNDS.SrnTone3.SoundSet),
                tone4 = new Tone(_vcf.SOUNDS.SrnTone4.AudioString, _elsVehicle, ToneType.SrnTon4, _vcf.SOUNDS.SrnTone4.AllowUse, soundSet: _vcf.SOUNDS.SrnTone4.SoundSet),
            };

            _mainSiren = new MainSiren(ref _tones);
            dual_siren = false;

            if (data != null) SetData(data);
            ElsUiPanel.SetUiDesc(_mainSiren.MainTones[_mainSiren.currentTone].Type, "SRN");
            ElsUiPanel.SetUiDesc("--", "HRN");
        }

        public void CleanUP(bool tooFarAwayCleanup = false)
        {
            if (_elsVehicle.Exists())
            {
                _elsVehicle.Vehicle.IsSirenActive = false;
            }
            if (!tooFarAwayCleanup)
            {
                _tones.horn.SetState(false);
                _tones.tone1.SetState(false);
                _tones.tone2.SetState(false);
                _tones.tone3.SetState(false);
                _tones.tone4.SetState(false);
            }
            
            _tones.horn.CleanUp();
            _tones.tone1.CleanUp();
            _tones.tone2.CleanUp();
            _tones.tone3.CleanUp();
            _tones.tone4.CleanUp();
        }

        internal void SyncUi()
        {
            
        }
    }
}