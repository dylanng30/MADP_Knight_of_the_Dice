using System.Collections;
using System.Collections.Generic;
using MADP.Models;
using UnityEngine;

namespace MADP.Services.GameSettings.Interfaces
{
    public interface IGameSettingsService
    {
        GameSettingsModel CurrentSettings { get; }
        void LoadSettings();
        void SaveSettings();

        //Actions
        void SetMasterVolume(float value);
        void SetMusicVolume(float value);
        void SetSfxVolume(float value);
        
        //WindowMode
        void SetFullScreen(bool isFullScreen);
        List<string> GetWindowModeOptions();
        
        //Resolution
        void SetResolution(int level);
        List<string> GetResolutionOptions();
    }
}
