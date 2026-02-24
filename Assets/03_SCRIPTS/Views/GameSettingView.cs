using System;
using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views
{
    public class GameSettingView : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle fullScreenToggle;

        [Header("Sound")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button saveButton;

        // Events gửi ra ngoài cho Controller
        public Action<float> OnMasterVolumeChanged;
        public Action<float> OnMusicVolumeChanged;
        public Action<float> OnSfxVolumeChanged;
        public Action<int> OnQualityChanged;
        public Action<bool> OnFullScreenChanged;
        public Action OnSaveClicked;
        public Action OnCloseClicked;

        private void Awake()
        {
            // Đăng ký sự kiện UI
            masterSlider.onValueChanged.AddListener(val => OnMasterVolumeChanged?.Invoke(val));
            musicSlider.onValueChanged.AddListener(val => OnMusicVolumeChanged?.Invoke(val));
            sfxSlider.onValueChanged.AddListener(val => OnSfxVolumeChanged?.Invoke(val));

            qualityDropdown.onValueChanged.AddListener(val => OnQualityChanged?.Invoke(val));
            fullScreenToggle.onValueChanged.AddListener(val => OnFullScreenChanged?.Invoke(val));

            saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
            closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void Initialize(GameSettingsModel currentSettings)
        {
            // Cập nhật UI theo dữ liệu hiện tại
            masterSlider.value = currentSettings.MasterVolume;
            musicSlider.value = currentSettings.MusicVolume;
            sfxSlider.value = currentSettings.SfxVolume;

            qualityDropdown.value = currentSettings.QualityLevel;
            fullScreenToggle.isOn = currentSettings.IsFullScreen;

            gameObject.SetActive(false); // Mặc định ẩn
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}