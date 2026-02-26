using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Services.GameSettings.Interfaces;
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
        
        private readonly Vector2Int[] _supportedResolutions = new Vector2Int[]
        {
            new Vector2Int(1920, 1080), // Full HD
            new Vector2Int(1600, 900), // HD+
            new Vector2Int(1366, 768), // Laptop HD
            new Vector2Int(1280, 720), // HD
        };

        private readonly string[] _windowModeOptions = new string[]
        {
            "Full Screen",
            "Windowed",
        };

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
            SetFullScreen(_model.IsFullScreen);
            ApplyResolutionInternal(_model.ResolutionIndex, _model.IsFullScreen);
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

        #region ---RESOLUTION---
        public List<string> GetResolutionOptions()
        {
            List<string> options = new List<string>();
            foreach (var res in _supportedResolutions)
            {
                options.Add($"{res.x} x {res.y}");
            }
            return options;
        }
        public void SetResolution(int resolutionIndex)
        {
            if (resolutionIndex < 0 || resolutionIndex >= _supportedResolutions.Length) return;

            _model.ResolutionIndex = resolutionIndex;
            ApplyResolutionInternal(resolutionIndex, _model.IsFullScreen);
        }
        #endregion
        
        #region ---WINDOW MODE---

        public List<string> GetWindowModeOptions() => _windowModeOptions.ToList();
        public void SetFullScreen(bool isFullScreen)
        {
            _model.IsFullScreen = isFullScreen;
            Screen.fullScreen = isFullScreen;
        }
        #endregion
        

        #region ---HELPER---
        private void SetMixerVolume(string paramName, float value)
        {
            if (_audioMixer == null) return;
            float db = value <= 0.001f ? MIN_DB : Mathf.Log10(value) * 20;
            _audioMixer.SetFloat(paramName, db);
        }
        private void ApplyResolutionInternal(int index, bool isFullScreen)
        {
            if (index < 0 || index >= _supportedResolutions.Length) index = 0;

            Vector2Int res = _supportedResolutions[index];
            Screen.SetResolution(res.x, res.y, isFullScreen);
        }
        #endregion

    }
}
