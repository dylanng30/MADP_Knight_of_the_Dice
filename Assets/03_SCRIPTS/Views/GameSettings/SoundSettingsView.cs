using System;
using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.GameSettings
{
    public class SoundSettingsView : MonoBehaviour
    {
        [Header("Master")]
        [SerializeField] private Toggle masterToggle;
        [SerializeField] private TextMeshProUGUI masterText;
        [SerializeField] private Slider masterSlider;
        
        public Action<float> OnMasterVolumeChanged;
        public Action<bool> OnMasterMuteChanged;
        
        [Header("SFX")]
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private TextMeshProUGUI sfxText;
        [SerializeField] private Slider sfxSlider;
        
        public Action<float> OnSfxVolumeChanged;
        public Action<bool> OnSfxMuteChanged;
        
        [Header("Music")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private TextMeshProUGUI musicText;
        [SerializeField] private Slider musicSlider;
        
        public Action<float> OnMusicVolumeChanged;
        public Action<bool> OnMusicMuteChanged;
        
        private void Awake()
        {
            masterSlider.onValueChanged.AddListener(val => OnMasterVolumeChanged?.Invoke(val));
            musicSlider.onValueChanged.AddListener(val => OnMusicVolumeChanged?.Invoke(val));
            sfxSlider.onValueChanged.AddListener(val => OnSfxVolumeChanged?.Invoke(val));
            
            masterToggle.onValueChanged.AddListener(isOn => OnMasterMuteChanged?.Invoke(!isOn));
            musicToggle.onValueChanged.AddListener(isOn => OnMusicMuteChanged?.Invoke(!isOn));
            sfxToggle.onValueChanged.AddListener(isOn => OnSfxMuteChanged?.Invoke(!isOn));
        }
        
        public void Setup(GameSettingsModel currentSettings)
        {
            masterSlider.SetValueWithoutNotify(currentSettings.MasterVolume);
            masterToggle.SetIsOnWithoutNotify(!currentSettings.IsMasterMuted);
            UpdateMasterUI(currentSettings.MasterVolume, currentSettings.IsMasterMuted);

            musicSlider.SetValueWithoutNotify(currentSettings.MusicVolume);
            musicToggle.SetIsOnWithoutNotify(!currentSettings.IsMusicMuted);
            UpdateMusicUI(currentSettings.MusicVolume, currentSettings.IsMusicMuted);

            sfxSlider.SetValueWithoutNotify(currentSettings.SfxVolume);
            sfxToggle.SetIsOnWithoutNotify(!currentSettings.IsSfxMuted);
            UpdateSfxUI(currentSettings.SfxVolume, currentSettings.IsSfxMuted);
        }
        
        public void UpdateMasterUI(float volume, bool isMuted)
        {
            masterText.text = isMuted ? "Tắt" : $"Overall Volume: {Mathf.RoundToInt(volume * 100)}%";
            masterSlider.interactable = !isMuted;
            masterToggle.SetIsOnWithoutNotify(!isMuted);
        }

        public void UpdateMusicUI(float volume, bool isMuted)
        {
            musicText.text = isMuted ? "Tắt" : $"SFX Volume: {Mathf.RoundToInt(volume * 100)}%";
            musicSlider.interactable = !isMuted;
            musicToggle.SetIsOnWithoutNotify(!isMuted);
        }

        public void UpdateSfxUI(float volume, bool isMuted)
        {
            sfxText.text = isMuted ? "Tắt" : $"Music Volume: {Mathf.RoundToInt(volume * 100)}%";
            sfxSlider.interactable = !isMuted;
            sfxToggle.SetIsOnWithoutNotify(!isMuted);
        }
    }
}