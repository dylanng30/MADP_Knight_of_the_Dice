using System;
using System.Collections;
using System.Collections.Generic;
using MADP.Managers;
using MADP.Models;
using MADP.Services;
using MADP.Services.Gold.Interfaces;
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
        }
        
        private void Start()
        {
            _botDecisionService = new BotDecisionService(boardController);

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
            yield return new WaitForSeconds(3f);
            UnitModel bestUnit = _botDecisionService.GetBestUnitToMove(CurrentTeam, CurrentDiceValue);

            if (bestUnit != null)
            {
                if (bestUnit.State == UnitState.InNest)
                {
                    boardController.SpawnUnit(bestUnit, EndTurn);
                }
                else
                {
                    var destCells = boardController.GetPotentialDestinationCell(bestUnit, CurrentDiceValue);
                    if (destCells.Count > 0)
                    {
                        CellModel target = destCells[0]; 
                        ExecuteMove(bestUnit, target); 
                    }
                    else
                    {
                        EndTurn();
                    }
                }
            }
            else
            {
                Debug.Log($"Bot {CurrentTeam} không có nước đi nào hợp lệ.");
                EndTurn();
            }
        }
    }
}