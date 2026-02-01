using System;
using MADP.Views.MainPanels.Interfaces;
using UnityEngine;

namespace MADP.Managers
{
    public class UIManager : MonoBehaviour
    {
        private GameManager _gameManager;
        
        private IMainPanel[] _mainPanels;

        private void Awake()
        {
            LoadMainPanels();
        }

        private void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
            _gameManager.OnGameStateChanged += ChangeMainPanel;
            
        }

        private void ChangeMainPanel(GameState currentGameState)
        {
            switch (currentGameState)
            {
                case GameState.MENU:
                    break;
                case GameState.PLAY:
                    break;
            }
        }

        private void Show<T>()
        {
            foreach (var panel in _mainPanels)
            {
                if(panel is T) panel.Show();
                else panel.Hide();
            }
        }

        private void LoadMainPanels()
        {
            
        }
    }
}