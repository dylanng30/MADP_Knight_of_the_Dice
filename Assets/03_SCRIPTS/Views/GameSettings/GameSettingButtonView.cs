using System;
using MADP.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.GameSettings
{
    public class GameSettingButtonView : MonoBehaviour
    {
        [SerializeField] private Button gameSettingsButton;
        private void OnEnable()
        {
            if (gameSettingsButton != null) gameSettingsButton.onClick.AddListener(ShowGameSettingsView);
        }

        private void OnDisable()
        {
            if (gameSettingsButton != null) gameSettingsButton.onClick.RemoveListener(ShowGameSettingsView);
        }

        private void ShowGameSettingsView()
        {
            if (GameSettingsController.Instance != null)
            {
                GameSettingsController.Instance.ShowGameSettings();
            }
        }
    }
}