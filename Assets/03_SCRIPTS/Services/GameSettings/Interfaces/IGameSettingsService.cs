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
        void SetMasterMute(bool isMuted);
        
        void SetMusicVolume(float value);
        void SetMusicMute(bool isMuted);
        
        void SetSfxVolume(float value);
        void SetSfxMute(bool isMuted);
        
        //WindowMode
        void SetFullScreen(bool isFullScreen);
        List<string> GetWindowModeOptions();
        
        //Resolution
        void SetResolution(int level);
        List<string> GetResolutionOptions();
    }
}
