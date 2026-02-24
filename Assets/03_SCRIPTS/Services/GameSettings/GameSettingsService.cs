using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MADP.Models;
using UnityEngine;
using UnityEngine.Audio;

namespace MADP.Services.GameSettings
{
    public class GameSettingsService : IGameSettingsService
    {
        private const string SAVE_KEY = "MADP_GameSettings";
        private const float MIN_DB = -80f;

        private GameSettingsModel _model;
        private readonly AudioMixer _audioMixer;

        public GameSettingsModel CurrentSettings => _model;

        public GameSettingsService(AudioMixer audioMixer)
        {
            _audioMixer = audioMixer;
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                _model = JsonUtility.FromJson<GameSettingsModel>(json);
            }
            else
            {
                _model = new GameSettingsModel();
            }

            ApplySettings();
        }

        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(_model);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        //Tuỳ chỉnh

        private void ApplySettings()
        {
            SetMasterVolume(_model.MasterVolume);
            SetMusicVolume(_model.MusicVolume);
            SetSfxVolume(_model.SfxVolume);
            SetQualityLevel(_model.QualityLevel);
            SetFullScreen(_model.IsFullScreen);
        }

        public void SetMasterVolume(float value)
        {
            _model.MasterVolume = value;
            SetMixerVolume("MasterVol", value);
        }

        public void SetMusicVolume(float value)
        {
            _model.MusicVolume = value;
            SetMixerVolume("MusicVol", value);
        }

        public void SetSfxVolume(float value)
        {
            _model.SfxVolume = value;
            SetMixerVolume("SFXVol", value);
        }

        public void SetQualityLevel(int level)
        {
            _model.QualityLevel = level;
            QualitySettings.SetQualityLevel(level, true);
        }

        public void SetFullScreen(bool isFullScreen)
        {
            _model.IsFullScreen = isFullScreen;
            Screen.fullScreen = isFullScreen;
        }

        #region ---HELPER---
        private void SetMixerVolume(string paramName, float value)
        {
            if (_audioMixer == null) return;
            float db = value <= 0.001f ? MIN_DB : Mathf.Log10(value) * 20;
            _audioMixer.SetFloat(paramName, db);
        }
        #endregion

    }
}
