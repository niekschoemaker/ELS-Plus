using CitizenFX.Core;
using ELS.configuration;
using ELS.NUI;
using Control = CitizenFX.Core.Control;

namespace ELS.Siren
{
    partial class Siren : IManagerEntry
    {
        int[] patts = { 0, 0, 0 };
        void AirHornLogic(bool pressed, bool disableControls = false)
        {
            if (disableControls)
            {
                Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Sound_Ahorn);
                Game.DisableControlThisFrame(2, Control.ScriptPadDown);
            }
            if (pressed)
            {
                patts[0] = _patternController.CurrentPrmPattern;
                patts[1] = _patternController.CurrentSecPattern;
                patts[2] = _patternController.CurrentWrnPattern;
                ElsUiPanel.ToggleUiBtnState(true, "HRN");
                ElsUiPanel.SetUiDesc("AH", "HRN");
                if (Global.BtnClicksBtwnHrnTones)
                {
                    ElsUiPanel.PlayUiSound("sirenclickoff");
                }
                if (_vcf.PRML.ForcedPatterns.MainHorn.Enabled)
                {
                    _patternController.CurrentPrmPattern = _vcf.PRML.ForcedPatterns.MainHorn.IntPattern;
                }
                if (_vcf.SECL.ForcedPatterns.MainHorn.Enabled)
                {
                    _patternController.CurrentSecPattern = _vcf.SECL.ForcedPatterns.MainHorn.IntPattern;
                }
                if (_vcf.WRNL.ForcedPatterns.MainHorn.Enabled)
                {
                    _patternController.CurrentWrnPattern = _vcf.WRNL.ForcedPatterns.MainHorn.IntPattern;
                }
                if (_vcf.SOUNDS.MainHorn.InterruptsSiren)
                {
                    if (_mainSiren._enable)
                    {
                        _mainSiren.interupted = true;
                        _mainSiren.SetEnable(false);
                    }
                }
                _tones.horn.SetState(true);
            }
            else
            {
                if (_vcf.PRML.ForcedPatterns.MainHorn.Enabled)
                {
                    _patternController.CurrentPrmPattern = patts[0];
                }
                if (_vcf.SECL.ForcedPatterns.MainHorn.Enabled)
                {
                    _patternController.CurrentSecPattern = patts[1];
                }
                if (_vcf.WRNL.ForcedPatterns.MainHorn.Enabled)
                {
                    _patternController.CurrentWrnPattern = patts[2];
                }
                ElsUiPanel.ToggleUiBtnState(false, "HRN");
                _tones.horn.SetState(false);
                if (_vcf.SOUNDS.MainHorn.InterruptsSiren &&
                    _mainSiren.interupted)
                {
                    _mainSiren.interupted = false;
                    _mainSiren.SetEnable(true);
                }
                ElsUiPanel.SetUiDesc("--", "HRN");
            }
        }

        bool SirenTone0Logic(bool pressed, bool disableControls = false)
        {
            if (!_tones.tone1.AllowUse)
            {
                return false;
            }
            if (disableControls) Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Snd_SrnTon1);
            if (!_mainSiren._enable || _mainSiren.currentTone == 0)
            {
                return false;
            }

            if (pressed)
            {
                _mainSiren.setMainTone(0);
                if (_vcf.PRML.ForcedPatterns.SrnTone1.Enabled)
                {
                    _patternController.CurrentPrmPattern = _vcf.PRML.ForcedPatterns.SrnTone1.IntPattern;
                }
                if (_vcf.SECL.ForcedPatterns.SrnTone1.Enabled)
                {
                    _patternController.CurrentSecPattern = _vcf.SECL.ForcedPatterns.SrnTone1.IntPattern;
                }
                if (_vcf.WRNL.ForcedPatterns.SrnTone1.Enabled)
                {
                    _patternController.CurrentWrnPattern = _vcf.WRNL.ForcedPatterns.SrnTone1.IntPattern;
                }
                if (dual_siren)
                {
                    _tones.tone4.SetState(false);
                    _tones.tone1.SetState(false);
                    _tones.tone2.SetState(true);
                    _tones.tone3.SetState(false);
                }
                ElsUiPanel.SetUiDesc(_mainSiren.MainTones[_mainSiren.currentTone].Type, "SRN");
                ElsUiPanel.ToggleUiBtnState(_mainSiren._enable, "SRN");
                if (Global.BtnClicksBtwnSrnTones)
                {
                    ElsUiPanel.PlayUiSound("sirenclickoff");
                }
                return true;
            }
            return false;

        }

        bool SirenTone1Logic(bool pressed, bool disableControls = false)
        {
            if (!_tones.tone2.AllowUse)
            {
                return false;
            }
            if (disableControls)
            {
                Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Snd_SrnTon2);
            }
            if (!_mainSiren._enable || _mainSiren.currentTone == 1)
            {
                return false;
            }

            if (pressed)
            {
                _mainSiren.setMainTone(1);
                if (_vcf.PRML.ForcedPatterns.SrnTone2.Enabled)
                {
                    _patternController.CurrentPrmPattern = _vcf.PRML.ForcedPatterns.SrnTone2.IntPattern;
                }
                if (_vcf.SECL.ForcedPatterns.SrnTone2.Enabled)
                {
                    _patternController.CurrentSecPattern = _vcf.SECL.ForcedPatterns.SrnTone2.IntPattern;
                }
                if (_vcf.WRNL.ForcedPatterns.SrnTone2.Enabled)
                {
                    _patternController.CurrentWrnPattern = _vcf.WRNL.ForcedPatterns.SrnTone2.IntPattern;
                }
                if (dual_siren)
                {
                    _tones.tone4.SetState(false);
                    _tones.tone1.SetState(false);
                    _tones.tone2.SetState(false);
                    _tones.tone3.SetState(true);
                }
                ElsUiPanel.SetUiDesc(_mainSiren.MainTones[_mainSiren.currentTone].Type, "SRN");
                ElsUiPanel.ToggleUiBtnState(_mainSiren._enable, "SRN");
                if (Global.BtnClicksBtwnSrnTones)
                {
                    ElsUiPanel.PlayUiSound("sirenclickoff");
                }
                return true;
            }
            return false;
        }

        bool SirenTone2Logic(bool pressed, bool disableControls = false)
        {
            if (!_tones.tone3.AllowUse)
            {
                return false;
            }
            if (disableControls)
            {
                Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Snd_SrnTon3);
            }
            if (!_mainSiren._enable || _mainSiren.currentTone == 2)
            {
                return false;
            }
            if (pressed)
            {
                _mainSiren.setMainTone(2);
                if (_vcf.PRML.ForcedPatterns.SrnTone3.Enabled)
                {
                    _patternController.CurrentPrmPattern = _vcf.PRML.ForcedPatterns.SrnTone3.IntPattern;
                }
                if (_vcf.SECL.ForcedPatterns.SrnTone3.Enabled)
                {
                    _patternController.CurrentSecPattern = _vcf.SECL.ForcedPatterns.SrnTone3.IntPattern;
                }
                if (_vcf.WRNL.ForcedPatterns.SrnTone3.Enabled)
                {
                    _patternController.CurrentWrnPattern = _vcf.WRNL.ForcedPatterns.SrnTone3.IntPattern;
                }
                if (dual_siren)
                {
                    _tones.tone3.SetState(false);
                    _tones.tone1.SetState(false);
                    _tones.tone2.SetState(false);
                    _tones.tone4.SetState(true);
                }
                ElsUiPanel.SetUiDesc(_mainSiren.MainTones[_mainSiren.currentTone].Type, "SRN");
                ElsUiPanel.ToggleUiBtnState(_mainSiren._enable, "SRN");
                if (Global.BtnClicksBtwnSrnTones)
                {
                    ElsUiPanel.PlayUiSound("sirenclickoff");
                }
                if (Global.BtnClicksBtwnSrnTones)
                {
                    ElsUiPanel.PlayUiSound("sirenclickoff");
                }
                return true;
            }

            return false;
        }

        bool SirenTone3Logic(bool pressed, bool disableControls = false)
        {
            if (!_tones.tone4.AllowUse)
            {
                return false;
            }
            if (disableControls)
            {
                Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Snd_SrnTon4);
            }
            if (!_mainSiren._enable || _mainSiren.currentTone == 3)
            {
                return false;
            }
            if (pressed)
            {
                _mainSiren.setMainTone(3);
                if (_vcf.PRML.ForcedPatterns.SrnTone4.Enabled)
                {
                    _patternController.CurrentPrmPattern = _vcf.PRML.ForcedPatterns.SrnTone4.IntPattern;
                }
                if (_vcf.SECL.ForcedPatterns.SrnTone4.Enabled)
                {
                    _patternController.CurrentSecPattern = _vcf.SECL.ForcedPatterns.SrnTone4.IntPattern;
                }
                if (_vcf.WRNL.ForcedPatterns.SrnTone4.Enabled)
                {
                    _patternController.CurrentWrnPattern = _vcf.WRNL.ForcedPatterns.SrnTone4.IntPattern;
                }
                
                if (dual_siren)
                {
                    _tones.tone4.SetState(true);
                    _tones.tone1.SetState(false);
                    _tones.tone2.SetState(false);
                    _tones.tone3.SetState(false);
                }
                ElsUiPanel.SetUiDesc(_mainSiren.MainTones[_mainSiren.currentTone].Type, "SRN");
                ElsUiPanel.ToggleUiBtnState(_mainSiren._enable, "SRN");
                if (Global.BtnClicksBtwnSrnTones)
                {
                    ElsUiPanel.PlayUiSound("sirenclickoff");
                }
                return true;
            }

            return false;
        }

        private void MainSirenToggleLogic(bool toggle, bool disableControls = false)
        {
            if (disableControls)
            {
                Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Toggle_SIRN);
            }

            if (toggle)
            {
                _mainSiren.SetEnable(!_mainSiren._enable);
                if (dual_siren)
                {
                    dual_siren = _mainSiren._enable;
                }
                ElsUiPanel.SetUiDesc(_mainSiren.MainTones[_mainSiren.currentTone].Type, "SRN");
                ElsUiPanel.PlayUiSound("sirenclickoff");
            }
            
        }

        void ManualSoundLogic(bool pressed, bool disableControls = false)
        {
            if (disableControls) Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Sound_Manul);
            if (pressed)
            {
                if (!_mainSiren._enable || (!_mainSiren._enable && _vcf.SOUNDS.MainHorn.InterruptsSiren &&
                                           _tones.horn.State))
                {
                    _tones.tone1.SetState(true);
                }
                else 
                {
                    _mainSiren.nextTone();
                }
                ElsUiPanel.ToggleUiBtnState(true, "MAN");
                if (Global.BtnClicksBtwnSrnTones)
                {
                    ElsUiPanel.PlayUiSound("sirenclickoff");
                }
            }
            else
            {
                if (!_mainSiren._enable || (!_mainSiren._enable && _vcf.SOUNDS.MainHorn.InterruptsSiren &&
                                           _tones.horn.State))
                {
                    _tones.tone1.SetState(false);
                }
                else
                {
                    _mainSiren.previousTone();
                }
                ElsUiPanel.ToggleUiBtnState(false, "MAN");
            }
        }

        void DualSirenLogic(bool toggle, bool disableControls = false)
        {
            if (disableControls) Game.DisableControlThisFrame(0, ElsConfiguration.KBBindings.Toggle_DSRN);
            if (toggle)
            {
                dual_siren = !dual_siren;
                switch(_mainSiren.MainTones[_mainSiren.currentTone].Type)
                {
                    case "WL":
                        _tones.tone2.SetState(dual_siren);
                        break;
                    case "YP":
                        _tones.tone3.SetState(dual_siren);
                        break;
                    case "A1":
                        _tones.tone4.SetState(dual_siren);
                        break;
                    case "A2":
                        _tones.tone1.SetState(dual_siren);
                        break;
                }
            }
            ElsUiPanel.ToggleUiBtnState(dual_siren, "DUAL");
            ElsUiPanel.PlayUiSound("sirenclickoff");
        }
    }
}