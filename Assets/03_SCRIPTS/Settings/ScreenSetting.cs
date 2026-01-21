using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScreenSetting : MonoBehaviour
{
    [SerializeField] private TMP_Text fullScreenState;
    [SerializeField] private GameObject resolutionButton;
    [SerializeField] private GameObject resolutionDropdown;
    [SerializeField] private TMP_Text resolutionText;
    [SerializeField] private List<Resolution> resolutions;
    private bool _isFullScreen;
    private int _currentWidthResolution;
    private int _currentHeightResolution;

    private void Start()
    {
        LoadResolution();
        FullScreen();
    }

    public void FullScreen()
    {
        _isFullScreen = !_isFullScreen;
        if (!_isFullScreen)
        {
            fullScreenState.text = "Tắt";
            Screen.SetResolution(_currentWidthResolution, _currentHeightResolution, FullScreenMode.Windowed);
            resolutionButton.SetActive(true);
        }
        else
        {
            fullScreenState.text = "Bật";
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
            resolutionButton.SetActive(false);
        }
    }

    public void Resolution()
    {
        resolutionDropdown.SetActive(true);
    }

    public void ChangeResolution(int idx)
    {
        Resolution res = resolutions[idx];
        resolutionText.text = res.width + "x" + res.height;
        _currentHeightResolution = res.height;
        _currentWidthResolution = res.width;
        if (!_isFullScreen)
        {
            Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed);
        }

        resolutionDropdown.SetActive(false);
    }

    void LoadResolution()
    {
        resolutions = new List<Resolution>
        {
            new Resolution { width = 1280, height = 720 },
            new Resolution { width = 1366, height = 768 },
            new Resolution { width = 1600, height = 900 },
            new Resolution { width = 1920, height = 1080 },
        };
        
        _currentWidthResolution = resolutions[0].width;
        _currentHeightResolution = resolutions[0].height;
    }
}