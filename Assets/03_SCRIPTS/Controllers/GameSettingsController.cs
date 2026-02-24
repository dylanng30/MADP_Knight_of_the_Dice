using MADP.Services;
using MADP.Services.GameSettings;
using MADP.Views;
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

            _view.Initialize(_service.CurrentSettings);

            _view.OnMasterVolumeChanged += _service.SetMasterVolume;
            _view.OnMusicVolumeChanged += _service.SetMusicVolume;
            _view.OnSfxVolumeChanged += _service.SetSfxVolume;

            _view.OnQualityChanged += _service.SetQualityLevel;
            _view.OnFullScreenChanged += _service.SetFullScreen;

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