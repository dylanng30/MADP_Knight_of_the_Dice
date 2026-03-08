using System;
using MADP.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views
{
    public class GameEndView : MonoBehaviour
    {
        [Header("PANELS")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject gameWinPanel;

        [Space(10)]
        [Header("SETTINGS")]
        [SerializeField] private Button returnToMenuButton;

        private void Awake()
        {
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.AddListener(ReturnToMenu);
            }
            if(gameWinPanel != null) gameWinPanel.SetActive(false);
            if(gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        public void Show(bool isWin)
        {
            gameObject.SetActive(false);
            if(gameWinPanel != null) gameWinPanel.SetActive(isWin);
            if(gameOverPanel != null) gameOverPanel.SetActive(!isWin);
        }

        public void Hide()
        {
            if(gameWinPanel != null) gameWinPanel.SetActive(false);
            if(gameOverPanel != null) gameOverPanel.SetActive(false);
            gameObject.SetActive(false);
        }

        private void ReturnToMenu()
        {
            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene("Menu");
            }
        }
    }
}