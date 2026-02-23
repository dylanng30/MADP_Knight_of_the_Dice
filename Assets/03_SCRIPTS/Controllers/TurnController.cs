using System;
using System.Collections;
using System.Collections.Generic;
using MADP.Managers;
using MADP.Models;
using MADP.Services;
using MADP.Services.AI;
using MADP.Services.AI.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Settings;
using MADP.States.TurnStates;
using MADP.States.TurnStates.Interfaces;
using MADP.Views;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MADP.Controllers
{
    public enum TurnState
    {
        Rolling, Choosing, WaitingForActions
    }
    public class TurnController : MonoBehaviour
    {
        [SerializeField] private BotProfileDatabaseSO botDB;
        [SerializeField] private BoardController boardController;
        [SerializeField] private DiceView diceView;
        [SerializeField] private UIManager _uiManager;
        
        private Dictionary<TurnState, ITurnState> _turnStates;
        public int CurrentDiceValue { get; private set; }
        
        private UnitModel _selectedUnit;
        private List<CellModel> _potentialDestination;
        private ITurnState _currentTurnState;
        
        //Services
        private IGoldService _goldService;
        private BotDecisionService _botDecisionService;
        
        
        private List<LobbySlotModel> _activeSlots = new List<LobbySlotModel>();
        private int _currentTeamIndex = 0;
        public TeamColor CurrentTeam => _activeSlots[_currentTeamIndex].TeamColor;
        public bool IsPlayerTurn => _activeSlots[_currentTeamIndex].PlayerType == PlayerType.Human;
        
        public void Initialize(
            IGoldService goldService, 
            List<LobbySlotModel> activeSlots)
        {
            _goldService = goldService;
            _activeSlots = activeSlots;
            
            _botDecisionService = new BotDecisionService();
            
            foreach (var slot in activeSlots)
            {
                if (slot.PlayerType == PlayerType.Bot)
                {
                    IBotBrain botBrain;
                    
                    BotProfileSO profile = botDB != null ? botDB.GetProfile(slot.BotType) : null;

                    if (profile != null)
                    {
                        botBrain = new SmartBotBrain(boardController, profile);
                    }
                    else
                    {
                        botBrain = new RandomBotBrain(boardController);
                    }
                    
                    _botDecisionService.RegisterBotStrategy(slot.TeamColor, botBrain);
                }
            }
        }
        
        private void Start()
        {
            LoadTurnStates();
            SwitchState(TurnState.Rolling);
        }
        private void Update()
        {
            _currentTurnState?.ExecuteTurn();
        }
        public void SwitchState(TurnState newState)
        {
            _currentTurnState?.ExitTurn();
            if (_turnStates.TryGetValue(newState, out var state))
            {
                _currentTurnState = state;
                _currentTurnState.EnterTurn();
            }
        }
        public void ShowDiceView(bool show)
        {
            diceView.gameObject.SetActive(show);
        }
        
        public void RollDice()
        {
            CurrentDiceValue = Random.Range(1, 7);
            Debug.Log($"{CurrentTeam} is rolling a {CurrentDiceValue}");
            diceView.Roll(CurrentDiceValue, OnDiceRollCompleted);
        }

        private void OnDiceRollCompleted()
        {
            bool canMoveAny = boardController.CheckIfAnyMovePossible(CurrentTeam, CurrentDiceValue);
            
            if (!canMoveAny)
            {
                _goldService.ApplyStuckBonus(CurrentTeam);
                EndTurn();
            }
            else
            {
                SwitchState(TurnState.Choosing);
            }
        }

        public void HandleUnitClicked(UnitModel unit)
        {
            if(unit.TeamOwner != CurrentTeam)
                return;
            
            boardController.SpawnUnit(unit, EndTurn);
        }
        
        public void HandleCellClicked(CellModel clickedCell)
        {
            if (_selectedUnit != null)
            {
                foreach (var cell in _potentialDestination)
                {
                    if (cell == clickedCell)
                    {
                        ExecuteMove(_selectedUnit, clickedCell);
                        return;
                    }
                }
            }
            
            if (clickedCell.HasUnit && clickedCell.Unit.TeamOwner == CurrentTeam)
            {
                if (boardController.CanInteract(clickedCell.Unit, CurrentDiceValue))
                {
                    SelectUnit(clickedCell.Unit);
                }
                else
                {
                    Debug.Log("Unit này không thể di chuyển (đang trong chuồng mà không có 6, hoặc bị chặn)");
                    DeselectCurrent();
                }
                return;
            }
            
            DeselectCurrent();
        }
        private void SelectUnit(UnitModel unit)
        {
            if (_selectedUnit != null) DeselectCurrent();

            _selectedUnit = unit;
            
            _potentialDestination = boardController.GetPotentialDestinationCell(unit, CurrentDiceValue);
            
            boardController.HighlightCells(_potentialDestination);
        }

        public void ShowUnitInfo(UnitModel unit)
        {
            
        }

        public void DeselectCurrent()
        {
            boardController.ClearAllHighlights();
            _selectedUnit = null;
            _potentialDestination = null;
        }

        private void ExecuteMove(UnitModel unitModel, CellModel destination)
        {
            boardController.MoveUnit(unitModel, destination, CurrentDiceValue, () => 
            {
                EndTurn();
            });
        }
        public void EndTurn()
        {
            if (CurrentDiceValue != 6)
            {
                _currentTeamIndex = (_currentTeamIndex + 1) % _activeSlots.Count;

                if(_currentTeamIndex == 0)
                {
                    _goldService.ApplyRoundBonus();
                }
            }
            
            _selectedUnit = null;
            SwitchState(TurnState.Rolling);
        }
        
        private void LoadTurnStates()
        {
            _turnStates = new Dictionary<TurnState, ITurnState>
            {
                { TurnState.Rolling, new RollingState(this) },
                { TurnState.Choosing, new ChoosingState(this) }
            };
        }
        
        //BOT - Temp

        public void HandleBotTurn()
        {
            StartCoroutine(BotThinkingProcecss());
        }
        private IEnumerator BotThinkingProcecss()
        {
            yield return new WaitForSeconds(1.5f);
            
            var bestMove = _botDecisionService.GetBestMove(CurrentTeam, CurrentDiceValue, boardController.Board);

            if (bestMove.Unit != null)
            {
                if (bestMove.Unit.State == UnitState.InNest)
                {
                    Debug.Log($"Bot {CurrentTeam} quyết định SINH QUÂN {bestMove.Unit.Id}");
                    boardController.SpawnUnit(bestMove.Unit, EndTurn);
                }
                else if (bestMove.Destination != null)
                {
                    Debug.Log($"Bot {CurrentTeam} di chuyển unit {bestMove.Unit.Id} đến {bestMove.Destination.Index}");
                    ExecuteMove(bestMove.Unit, bestMove.Destination); 
                }
                else
                {
                    EndTurn();
                }
            }
            else
            {
                Debug.Log($"Bot {CurrentTeam} không có nước đi nào hợp lệ (Bị chặn hoặc kẹt).");
                EndTurn();
            }
        }
    }
}