using System;
using MADP.Controllers;
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
    public class GameManager : Singleton<GameManager>
    {
        [Header("---MANAGERS---")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private GoldUIManager _goldUIManager;
        
        [Header("---CONTROLLERS---")]
        [SerializeField] private BoardController _boardController;
        [SerializeField] private TurnController _turnController;
        
        private IGoldService _goldService;
        private IPathfindingService _pathfindingService;
        private ICombatService _combatService;
        private ICellEventService _cellEventService;
        
        private GameState _currentGameState;
        public Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            _goldService = new GoldService();
            _pathfindingService = new PathfindingService();
            _combatService = new CombatService();
            _cellEventService = new CellEventService(_goldService);
            
            _boardController.Initialize(_goldService, _pathfindingService, _combatService, _cellEventService);
            _turnController.Initialize(_goldService);
            _goldUIManager.Initialize(_goldService);
            
            _goldService.Initialize(Constants.InitialGold);
        }

        private void Start()
        {
            _currentGameState = GameState.MENU;
            ChangeState(_currentGameState);
        }

        public void ChangeState(GameState newState)
        {
            _currentGameState = newState;
            OnGameStateChanged?.Invoke(_currentGameState);
        }
    }
}

