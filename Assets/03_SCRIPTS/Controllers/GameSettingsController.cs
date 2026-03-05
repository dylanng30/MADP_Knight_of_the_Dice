using System;
using MADP.Services.GameSettings;
using MADP.Services.GameSettings.Interfaces;
using MADP.Utilities;
using MADP.Views.GameSettings;
using UnityEngine;
using UnityEngine.Audio;

namespace MADP.Controllers
{
    public class GameSettingsController : PersistentSingleton<GameSettingsController>
    {
        [SerializeField] private GameSettingView _view;
        [SerializeField] private AudioMixer _audioMixer;

        private IGameSettingsService _service;

        protected override void Awake()
        {
            base.Awake();
            _service = new GameSettingsService(_audioMixer);
            
            _view.SoundPanel.OnMasterVolumeChanged += HandleMasterVolumeChanged;
            _view.SoundPanel.OnMusicVolumeChanged += HandleMusicVolumeChanged;
            _view.SoundPanel.OnSfxVolumeChanged += HandleSfxVolumeChanged;
            
            _view.SoundPanel.OnMasterMuteChanged += HandleMasterMuteChanged;
            _view.SoundPanel.OnMusicMuteChanged += HandleMusicMuteChanged;
            _view.SoundPanel.OnSfxMuteChanged += HandleSfxMuteChanged;

            _view.GeneralPanel.OnResolutionChanged += _service.SetResolution;
            _view.GeneralPanel.OnWindowModeChanged += _service.SetFullScreen;

            _view.OnSaveClicked += HandleSave;
            _view.OnCloseClicked += HideGameSettings;
        }

        private void Start()
        {
            _service.LoadSettings();
            
            var resolutionOptions = _service.GetResolutionOptions();
            var windowModeOptions = _service.GetWindowModeOptions();
            
            _view.Initialize(_service.CurrentSettings);
            _view.GeneralPanel.Setup(_service.CurrentSettings, resolutionOptions, windowModeOptions);
        }

        #region --- HANDLERS ---
        //MASTER
        private void HandleMasterVolumeChanged(float volume)
        {
            _service.SetMasterVolume(volume);
            _view.SoundPanel.UpdateMasterUI(volume, _service.CurrentSettings.IsMasterMuted);
        }
        private void HandleMasterMuteChanged(bool isMuted)
        {
            _service.SetMasterMute(isMuted);
            var currentSettings = _service.CurrentSettings;
            _view.SoundPanel.UpdateMasterUI(currentSettings.MasterVolume, currentSettings.IsMasterMuted);
            _view.SoundPanel.UpdateMusicUI(currentSettings.MusicVolume, currentSettings.IsMusicMuted);
            _view.SoundPanel.UpdateSfxUI(currentSettings.SfxVolume, currentSettings.IsSfxMuted);
        }
        
        //MUSIC
        private void HandleMusicVolumeChanged(float volume)
        {
            _service.SetMusicVolume(volume);
            _view.SoundPanel.UpdateMusicUI(volume, _service.CurrentSettings.IsMusicMuted);
        }
        private void HandleMusicMuteChanged(bool isMuted)
        {
            _service.SetMusicMute(isMuted);
            _view.SoundPanel.UpdateMusicUI(_service.CurrentSettings.MusicVolume, isMuted);
        }

        //SFX
        private void HandleSfxVolumeChanged(float volume)
        {
            _service.SetSfxVolume(volume);
            _view.SoundPanel.UpdateSfxUI(volume, _service.CurrentSettings.IsSfxMuted);
        }
        private void HandleSfxMuteChanged(bool isMuted)
        {
            _service.SetSfxMute(isMuted);
            _view.SoundPanel.UpdateSfxUI(_service.CurrentSettings.SfxVolume, isMuted);
        }
        #endregion

        private void HandleSave()
        {
            _service.SaveSettings();
            _view.Hide();
        }

        public void ShowGameSettings()
        {
            _view.Show();
        }

        public void HideGameSettings()
        {
            _view.Hide();
        }
    }
}