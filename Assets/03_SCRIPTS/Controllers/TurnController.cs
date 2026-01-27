using System;
using System.Collections;
using System.Collections.Generic;
using MADP.Managers;
using MADP.Models;
using MADP.Services;
using MADP.States.TurnStates;
using MADP.States.TurnStates.Interfaces;
using MADP.Views;
using UnityEngine;

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
        
        private readonly TeamColor[] _turnOrder = { 
            TeamColor.Red, TeamColor.Green, TeamColor.Yellow, TeamColor.Blue 
        };
        private DiceService _diceService;
        private BotDecisionService _botDecisionService;
        private Dictionary<TurnState, ITurnState> _turnStates;

        public TeamColor CurrentTeam => _turnOrder[_currentTeamIndex];
        public TurnState CurrentState { get; private set; }
        
        public int CurrentDiceValue { get; private set; }
        
        private TeamColor PlayerColor;
        private int _currentTeamIndex = 0;
        public bool IsPlayerTurn => PlayerColor == _turnOrder[_currentTeamIndex];
        
        private CellModel _lastChosenCell;
        
        private UnitModel _selectedUnit;
        private List<CellModel> _potentialDestination;
        private ITurnState _currentTurnState;
        
        private void Start()
        {
            _diceService = new DiceService();
            _botDecisionService = new BotDecisionService(boardController);
            PlayerColor = TeamColor.Red;
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
            CurrentDiceValue = _diceService.Roll();
            Debug.Log($"{CurrentTeam} is rolling a {CurrentDiceValue}");
            diceView.Roll(CurrentDiceValue, OnDiceRollCompleted);
        }

        private void OnDiceRollCompleted()
        {
            bool canMoveAny = boardController.CheckIfAnyMovePossible(CurrentTeam, CurrentDiceValue);
            
            if (!canMoveAny)
            {
                //Debug.Log($"Team {CurrentTeam} không đi được quân nào với {CurrentDiceValue} điểm -> End Turn.");
                EndTurn();
            }
            else
            {
                SwitchState(TurnState.Choosing);
            }
        }

        public void HandleUnitClicked(UnitModel unit)
        {
            if(unit.TeamOwner != PlayerColor)
                return;
            
            boardController.SpawnUnit(unit, EndTurn);
        }
        
        public void HandleCellClicked(CellModel clickedCell)
        {
            // TRƯỜNG HỢP 1: Click vào ô Đích (để di chuyển)
            // Điều kiện: Đang có Unit được chọn VÀ Click đúng vào ô đích đã tính toán
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

            // TRƯỜNG HỢP 2: Click vào Quân mình (để chọn)
            if (clickedCell.HasUnit && clickedCell.Unit.TeamOwner == CurrentTeam)
            {
                // Kiểm tra quân này có đi được với xúc xắc hiện tại không
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

            // TRƯỜNG HỢP 3: Click lung tung
            DeselectCurrent();
        }
        private void SelectUnit(UnitModel unit)
        {
            // 1. Reset selection cũ
            if (_selectedUnit != null) DeselectCurrent();

            _selectedUnit = unit;

            // 2. Tính toán đích đến
            _potentialDestination = boardController.GetPotentialDestinationCell(unit, CurrentDiceValue);

            // 3. Highlight ô đích
            boardController.HighlightCells(_potentialDestination);

            // 4. Show UI Info (Gọi UIManager)
            // _uiManager.ShowUnitInfo(unit);
            
            //Debug.Log($"Đã chọn Unit {unit.Id}. Click vào ô {_potentialDestination?.Index} để di chuyển.");
        }

        public void DeselectCurrent()
        {
            // Ẩn Highlight
            boardController.ClearAllHighlights();
            
            // Ẩn UI Info
            // _uiManager.HideUnitInfo();

            _selectedUnit = null;
            _potentialDestination = null;
        }

        private void ExecuteMove(UnitModel unitModel, CellModel destination)
        {
            //DeselectCurrent(); // Dọn dẹp highlight trước khi đi
            
            boardController.MoveUnit(unitModel, destination, CurrentDiceValue, () => 
            {
                EndTurn();
            });
        }
        public void EndTurn()
        {
            if (CurrentDiceValue != 6)
            {
                _currentTeamIndex = (_currentTeamIndex + 1) % _turnOrder.Length;
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
                
                        Debug.Log($"Bot {CurrentTeam} di chuyển unit {bestUnit.Id} đến {target.Index}");
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