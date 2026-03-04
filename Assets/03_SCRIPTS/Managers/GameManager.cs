using System;
using MADP.Controllers;
using MADP.Models;
using MADP.Services.CellEvent;
using MADP.Services.CellEvent.Interfaces;
using MADP.Services.Combat;
using MADP.Services.Combat.Interfaces;
using MADP.Services.Gold;
using MADP.Services.Gold.Interfaces;
using MADP.Services.Pathfinding;
using MADP.Services.Pathfinding.Interfaces;
using MADP.Utilities;
using UnityEngine;

namespace MADP.Managers
{
    public enum GameState
    {
        MENU, PLAY
    }
    public class GameManager : PersistentSingleton<GameManager>
    {
        [Header("---MANAGERS---")]
        [SerializeField] private UIManager _uiManager;
        
        private GameState _currentGameState;
        public Action<GameState> OnGameStateChanged;
        
        public MatchSettingsModel CurrentMatchSettings { get; set; }

        private void Start()
        {
            //_currentGameState = GameState.MENU;
            //ChangeState(_currentGameState);
        }

        public void ChangeState(GameState newState)
        {
            _currentGameState = newState;
            OnGameStateChanged?.Invoke(_currentGameState);
        }
    }
}

