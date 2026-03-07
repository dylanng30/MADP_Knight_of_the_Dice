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
using MADP.Systems;
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
        [Space(10)]
        [SerializeField] private BotProfileDatabaseSO botDB;
        [SerializeField] private BoardController boardController;
        [SerializeField] private UIManager _uiManager;
        
        public int CurrentDiceValue { get; private set; }
        
        private UnitModel _selectedUnit;
        private List<CellModel> _potentialDestination;
        private ITurnState _currentTurnState;
        
        //Services
        private IGoldService _goldService;
        private BotDecisionService _botDecisionService;
        
        private List<LobbySlotModel> _activeSlots = new ();
        private Dictionary<TurnState, ITurnState> _turnStates;
        private DiceView _diceView;
        
        [Header("Phase Settings")]
        [SerializeField] private ShoppingPhaseController shoppingPhaseController;
        private int _currentRound = 1;
        private const int SHOPPING_PHASE_INTERVAL = 5;
        
        [Header("Turn Settings")]
        [SerializeField] private TurnView turnView;
        private int _currentTeamIndex = 0;
        private float _timePerTurn;
        private float _currentTurnTimer;
        
        [Header("UI Settings")]
        [SerializeField] private RollButtonView rollButtonView;
        [SerializeField] private EndTurnButtonView endTurnButtonView;
        
        public float CurrentTurnTimer => _currentTurnTimer;
        
        public TeamColor CurrentTeam => _activeSlots[_currentTeamIndex].TeamColor;
        public bool IsPlayerTurn => _activeSlots[_currentTeamIndex].PlayerType == PlayerType.Human;
        
        //Events
        public Action<int> OnDiceRolled;
        //public event Action<TeamColor> OnTurnChanged;
        
        private void Start()
        {
            if (rollButtonView != null) rollButtonView.OnRollClicked += HandleInteractInput;
            if (endTurnButtonView != null) endTurnButtonView.OnEndClicked += HandleInteractInput;
        }
        private void OnDestroy()
        {
            if (rollButtonView != null) rollButtonView.OnRollClicked -= HandleInteractInput;
            if (endTurnButtonView != null) endTurnButtonView.OnEndClicked -= HandleInteractInput;
        }
        
        public void Initialize(
            IGoldService goldService, 
            List<LobbySlotModel> activeSlots, 
            DiceView diceView,
            float timePerTurn)
        {
            _goldService = goldService;
            _activeSlots = activeSlots;
            _diceView = diceView;
            _timePerTurn = timePerTurn;
            
            _botDecisionService = new BotDecisionService();
            
            foreach (var slot in activeSlots)
            {
                if (slot.PlayerType == PlayerType.Bot)
                {
                    IBotBrain botBrain;
                    
                    BotProfileSO profile = botDB != null ? botDB.GetProfile(slot.BotType) : null;
                    botBrain = profile != null ? new SmartBotBrain(boardController, profile) : new RandomBotBrain(boardController);
                    //Debug.Log($"Bot team {slot.TeamColor}: {slot.BotType} / {botBrain.GetType().Name}");
                    _botDecisionService.RegisterBotStrategy(slot.TeamColor, botBrain);
                }
            }
            
            LoadTurnStates();
            _currentTeamIndex = 0;
            StartTurnProcess();

            Time.timeScale = 5;
        }
        
        private void Update()
        {
            _currentTurnState?.ExecuteTurn();
            
            if (!ActionSystem.Instance.IsPerforming)
            {
                if (_currentTurnTimer > 0)
                {
                    _currentTurnTimer -= Time.deltaTime;
                    turnView.UpdateTimer(_currentTurnTimer);

                    if (_currentTurnTimer <= 0)
                    {
                        Debug.Log($"[Time Out] Đội {CurrentTeam} đã hết thời gian lượt đấu! Tự động qua lượt.");
                        EndTurn();
                    }
                }
            }
        }
        
        private void HandleInteractInput()
        {
            if (IsPlayerTurn && _currentTurnState != null && !ActionSystem.Instance.IsPerforming)
            {
                _currentTurnState.OnInteract();
            }
        }
        
        public void SetRollButtonVisibility(bool isVisible)
        {
            if (rollButtonView) 
            {
                rollButtonView.SetActive(isVisible);
                rollButtonView.SetInteractable(isVisible);
            }
        }

        public void SetEndTurnButtonVisibility(bool isVisible)
        {
            if (endTurnButtonView)
            {
                endTurnButtonView.SetActive(isVisible);
                endTurnButtonView.SetInteractable(isVisible);
            }
        }
        
        private void StartTurnProcess()
        {
            _currentTurnState?.ExitTurn();
            _currentTurnState = null; 
    
            DeselectCurrent();
            
            _currentTurnTimer = _timePerTurn;
            turnView.UpdateTimer(_currentTurnTimer);
            
            //OnTurnChanged?.Invoke(CurrentTeam);
            turnView.AnimateTurnNotification(CurrentTeam, IsPlayerTurn, () => 
            {
                SwitchState(TurnState.Rolling);
            });
        }
        private void OnTurnActionCompleted()
        {
            if (boardController.CheckWinCondition(CurrentTeam))
            {
                SwitchState(TurnState.WaitingForActions);
                return;
            }
            
            if (CurrentDiceValue == 6 && IsPlayerTurn)
            {
                DeselectCurrent();
            }
            
            EndTurn();
        }
        public void SwitchState(TurnState newState)
        {
            _currentTurnState?.ExitTurn();
            
            if (_turnStates.TryGetValue(newState, out var state))
            {
                SetRollButtonVisibility(newState == TurnState.Rolling && IsPlayerTurn);
                SetEndTurnButtonVisibility(newState == TurnState.Choosing && IsPlayerTurn);
                
                _currentTurnState = state;
                _currentTurnState.EnterTurn();
            }
        }
        
        public void RollDice()
        {
            CurrentDiceValue = Random.Range(1, 7);
            CurrentDiceValue = CurrentDiceValue <= 3 ? 1 : 6;
            _diceView.Roll(CurrentDiceValue, OnDiceRollCompleted);
            
            SetRollButtonVisibility(false);
        }

        private void OnDiceRollCompleted()
        {
            OnDiceRolled?.Invoke(CurrentDiceValue);
            
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
            
            SetEndTurnButtonVisibility(false);
            boardController.SpawnUnit(unit, OnTurnActionCompleted);
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
                        DeselectCurrent();
                        return;
                    }
                }
            }
            
            if (clickedCell.HasUnit && clickedCell.Unit.TeamOwner == CurrentTeam)
            {
                if (boardController.CanInteract(clickedCell.Unit, CurrentDiceValue))
                {
                    SelectUnit(clickedCell.Unit);
                    return;
                }
            }
            
            DeselectCurrent();
        }
        private void SelectUnit(UnitModel unit)
        {
            if (_selectedUnit != null) DeselectCurrent();

            _selectedUnit = unit;
            
            _potentialDestination = boardController.GetPotentialDestinationCell(unit, CurrentDiceValue);
            boardController.HighlightCells(_potentialDestination);
            
            CellModel currentCell = boardController.GetCurrentCellOfUnit(unit);
            if (currentCell != null)
            {
                boardController.HighlightSelection(currentCell);
            }
        }

        public void DeselectCurrent()
        {
            boardController.ClearAllHighlights();
            
            _selectedUnit = null;
            _potentialDestination = null;
        }

        private void ExecuteMove(UnitModel unitModel, CellModel destination)
        {
            SetEndTurnButtonVisibility(false);
            boardController.MoveUnit(unitModel, destination, CurrentDiceValue, OnTurnActionCompleted);
        }
        public void EndTurn()
        {
            //Reset
            _selectedUnit = null;
            OnDiceRolled?.Invoke(0);
            
            if (CurrentDiceValue != 6)
            {
                _currentTeamIndex = (_currentTeamIndex + 1) % _activeSlots.Count;

                if(_currentTeamIndex == 0)
                {
                    _currentRound++;
                    _goldService.ApplyRoundBonus();
                    /*if (_currentRound % SHOPPING_PHASE_INTERVAL == 0)
                    {
                        turnView.AnimateShopPhaseNotification();
                        shoppingPhaseController.ShowPhaseShop(EndTurn);
                        return;
                    }*/
                }
            }
            
            
            StartTurnProcess();
        }
        
        private void LoadTurnStates()
        {
            _turnStates = new Dictionary<TurnState, ITurnState>
            {
                { TurnState.Rolling, new RollingState(this) },
                { TurnState.Choosing, new ChoosingState(this) },
                { TurnState.WaitingForActions, new WinState(this) }
            };
        }
        
        //BOT - Temp
        public void HandleBotTurn()
        {
            StartCoroutine(BotThinkingProcecss());
        }
        private IEnumerator BotThinkingProcecss()
        {
            yield return new WaitForSeconds(1f);
            
            var bestMove = _botDecisionService.GetBestMove(CurrentTeam, CurrentDiceValue, boardController.Board);

            if (bestMove.Unit != null)
            {
                if (bestMove.Unit.State == UnitState.InNest)
                {
                    Debug.Log($"Bot {CurrentTeam} quyết định sinh quân {bestMove.Unit.Id}");
                    boardController.SpawnUnit(bestMove.Unit, EndTurn);
                }
                else if (bestMove.Destination != null)
                {
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