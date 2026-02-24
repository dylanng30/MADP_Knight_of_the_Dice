using System;
using System.Collections.Generic;
using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.GameSettings
{
    public class GeneralSettingsView : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown windowModeDropdown;
        
        public Action<int> OnResolutionChanged;
        public Action<bool> OnWindowModeChanged;

        private void Awake()
        {
            resolutionDropdown.onValueChanged.AddListener(val =>  OnResolutionChanged(val));
            windowModeDropdown.onValueChanged.AddListener(val =>  OnWindowModeChanged(val == 0));
        }

        public void Setup(GameSettingsModel currentSettings, List<string> resolutionOptions, List<string> windowModeOptions)
        {
            resolutionDropdown.AddOptions(resolutionOptions);
            windowModeDropdown.AddOptions(windowModeOptions);
            
            resolutionDropdown.value = currentSettings.ResolutionIndex;
            windowModeDropdown.value = currentSettings.IsFullScreen ? 0 : 1;
        }
    }
}