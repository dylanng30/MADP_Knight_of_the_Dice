using System;
using MADP.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.GameSettings
{
    public class SoundSettingsView : MonoBehaviour
    {
        [Header("Sound")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        
        public Action<float> OnMasterVolumeChanged;
        public Action<float> OnMusicVolumeChanged;
        public Action<float> OnSfxVolumeChanged;
        
        private void Awake()
        {
            //masterSlider.onValueChanged.AddListener(val => OnMasterVolumeChanged?.Invoke(val));
            //musicSlider.onValueChanged.AddListener(val => OnMusicVolumeChanged?.Invoke(val));
            //sfxSlider.onValueChanged.AddListener(val => OnSfxVolumeChanged?.Invoke(val));
        }
        
        public void Setup(GameSettingsModel currentSettings)
        {
            //masterSlider.value = currentSettings.MasterVolume;
            //musicSlider.value = currentSettings.MusicVolume;
            //sfxSlider.value = currentSettings.SfxVolume;
        }
    }
}