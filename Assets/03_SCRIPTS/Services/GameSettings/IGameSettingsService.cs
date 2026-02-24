using System.Collections;
using System.Collections.Generic;
using MADP.Models;
using UnityEngine;

namespace MADP.Services
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
        void SetQualityLevel(int level);
        void SetFullScreen(bool isFullScreen);
    }
}
