using MADP.Services.GameSettings;
using MADP.Services.GameSettings.Interfaces;
using MADP.Views.GameSettings;
using UnityEngine;
using UnityEngine.Audio;

namespace MADP.Controllers
{
    public class GameSettingsController : MonoBehaviour
    {
        [SerializeField] private GameSettingView _view;
        [SerializeField] private AudioMixer _audioMixer;

        private IGameSettingsService _service;

        public void Initialize()
        {
            _service = new GameSettingsService(_audioMixer);
            var resolutionOptions = _service.GetResolutionOptions();
            var windowModeOptions = _service.GetWindowModeOptions();

            _view.Initialize(_service.CurrentSettings);
            
            _view.GeneralPanel.Setup(_service.CurrentSettings, resolutionOptions, windowModeOptions);

            _view.SoundPanel.OnMasterVolumeChanged += _service.SetMasterVolume;
            _view.SoundPanel.OnMusicVolumeChanged += _service.SetMusicVolume;
            _view.SoundPanel.OnSfxVolumeChanged += _service.SetSfxVolume;

            _view.GeneralPanel.OnResolutionChanged += _service.SetResolution;
            _view.GeneralPanel.OnWindowModeChanged += _service.SetFullScreen;

            _view.OnSaveClicked += HandleSave;
            _view.OnCloseClicked += _view.Hide;
        }

        private void HandleSave()
        {
            _service.SaveSettings();
            _view.Hide();
        }

        public void OpenSettings()
        {
            _view.Show();
        }
    }
}