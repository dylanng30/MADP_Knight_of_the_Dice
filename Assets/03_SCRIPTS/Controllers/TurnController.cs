using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MADP.Managers;
using MADP.Models;
using MADP.Services;
using MADP.Services.AI;
using MADP.Services.AI.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Services.Inventory.Interfaces;
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
        Rolling,
        Choosing,
        Shopping,
        WaitingForActions
    }

    public class TurnController : MonoBehaviour
    {
        [Space(10)] [SerializeField] private BotProfileDatabaseSO botDB;
        [SerializeField] private BoardController boardController;
        [SerializeField] private UIManager _uiManager;

        public int CurrentDiceValue { get; private set; }

        private UnitModel _selectedUnit;
        private List<CellModel> _potentialDestination;
        private ITurnState _currentTurnState;

        //Services
        private IGoldService _goldService;
        public IGoldService GoldService => _goldService;

        private BotDecisionService _botDecisionService;
        private IItemService _itemService;
        public IItemService ItemService => _itemService;

        private List<LobbySlotModel> _activeSlots = new();
        public IReadOnlyList<LobbySlotModel> ActiveSlots => _activeSlots;

        private List<TeamColor> _finishedTeams = new();
        public IReadOnlyList<TeamColor> FinishedTeams => _finishedTeams;

        private Dictionary<TurnState, ITurnState> _turnStates;
        private DiceView _diceView;

        [Header("Phase Settings")] private ShoppingPhaseController _shoppingController;
        private int _currentRound = 1;
        public int CurrentRound => _currentRound;
        private const int SHOPPING_PHASE_INTERVAL = 5;

        [Header("Turn Settings")] [SerializeField]
        private TurnView turnView;

        private int _currentTeamIndex = 0;
        private float _timePerTurn;
        private float _currentTurnTimer;

        [Header("UI Settings")] [SerializeField]
        private RollButtonView rollButtonView;

        [SerializeField] private EndTurnButtonView endTurnButtonView;
        [SerializeField] private GameEndView gameEndView;

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
            ShoppingPhaseController shoppingController,
            IGoldService goldService,
            BotDecisionService botDecisionService,
            IItemService itemService,
            List<LobbySlotModel> activeSlots,
            DiceView diceView,
            float timePerTurn)
        {
            _shoppingController = shoppingController;

            _goldService = goldService;
            _botDecisionService = botDecisionService;
            _itemService = itemService;

            _activeSlots = activeSlots;
            _diceView = diceView;
            _timePerTurn = timePerTurn;

            foreach (var slot in activeSlots)
            {
                if (slot.PlayerType == PlayerType.Bot)
                {
                    IBotBrain botBrain;

                    BotProfileSO profile = botDB != null ? botDB.GetProfile(slot.BotType) : null;
                    botBrain = profile != null
                        ? new SmartBotBrain(boardController, profile)
                        : new RandomBotBrain(boardController);
                    //Debug.Log($"Bot team {slot.TeamColor}: {slot.BotType} / {botBrain.GetType().Name}");
                    _botDecisionService.RegisterBotStrategy(slot.TeamColor, botBrain);
                }
            }

            _finishedTeams.Clear();
            LoadTurnStates();
            _currentTeamIndex = 0;
            StartTurnProcess();

            //Time.timeScale = 5;
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
            turnView.AnimateTurnNotification(CurrentTeam, IsPlayerTurn, () => { SwitchState(TurnState.Rolling); });
        }

        private void OnTurnActionCompleted()
        {
            if (boardController.CheckWinCondition(CurrentTeam))
            {
                if (!isTrainingMode)
                {
                    // Chơi bình thường: 1 đội về đích là game kết thúc ngay lập tức
                    SwitchState(TurnState.WaitingForActions);
                    return;
                }

                // Training Mode: Tiếp tục đua xếp hạng cho ML-Agent
                if (!_finishedTeams.Contains(CurrentTeam))
                {
                    _finishedTeams.Add(CurrentTeam);
                    Debug.Log($"Team {CurrentTeam} đã về đích! Hạng {_finishedTeams.Count}");
                }

                if (_finishedTeams.Count >= _activeSlots.Count - 1)
                {
                    // Nếu chỉ còn 1 đội cuối cùng (hoặc đã xong hết), gán đội cuối cùng hạng chót
                    if (_finishedTeams.Count == _activeSlots.Count - 1)
                    {
                        var lastTeam = _activeSlots.Select(s => s.TeamColor).Except(_finishedTeams).FirstOrDefault();
                        if (lastTeam != TeamColor.None) _finishedTeams.Add(lastTeam);
                    }

                    TriggerGameEnd(_finishedTeams[0]);
                    return;
                }
            }

            if (CurrentDiceValue == 6 && IsPlayerTurn && !_finishedTeams.Contains(CurrentTeam))
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
            //CurrentDiceValue = CurrentDiceValue <= 3 ? 1 : 6;
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
            if (unit.TeamOwner != CurrentTeam)
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

            if (CurrentDiceValue != 6 || _finishedTeams.Contains(CurrentTeam))
            {
                do
                {
                    _currentTeamIndex = (_currentTeamIndex + 1) % _activeSlots.Count;

                    if (_currentTeamIndex == 0)
                    {
                        _currentRound++;
                        _goldService.ApplyRoundBonus();

                        if (isTrainingMode && _currentRound > maxRound)
                        {
                            Debug.Log($"[Timeout] Game reached maxRound {maxRound}. Ending episode.");
                            
                            // Kết thúc game với danh sách rỗng để báo hiệu Timeout/Hòa
                            // Agent sẽ nhận reward -0.5 (theo logic HandleGameRanked trong TeamAgent)
                            TriggerGameEnd(TeamColor.None);
                            return;
                        }

                        if (_currentRound % SHOPPING_PHASE_INTERVAL == 0)
                        {
                            SwitchState(TurnState.Shopping);
                            return;
                        }
                    }
                } while (_finishedTeams.Contains(CurrentTeam) && _finishedTeams.Count < _activeSlots.Count);
            }

            StartTurnProcess();
        }

        //SHOPPING PHASE
        public void StartShoppingPhase()
        {
            turnView.AnimateTurnNotification(TeamColor.None, false, null);
            _shoppingController.ShowPhaseShop(OnShoppingPhaseCompleted);
        }

        private void OnShoppingPhaseCompleted()
        {
            StartTurnProcess();
        }

        private void LoadTurnStates()
        {
            _turnStates = new Dictionary<TurnState, ITurnState>
            {
                { TurnState.Rolling, new RollingState(this) },
                { TurnState.Choosing, new ChoosingState(this) },
                { TurnState.Shopping, new ShoppingState(this) },
                { TurnState.WaitingForActions, new WinState(this) }
            };
        }

        #region ML-Agent Integration

        [Header("ML-Agent Settings")]
        [SerializeField] public bool isTrainingMode;
        [SerializeField] private int maxRound = 500;
        public int MaxRound => maxRound;

        public Action<TeamColor> OnMLAgentTurn;
        public Action<TeamColor, List<ItemDataSO>> OnMLAgentShoppingTurn;

        public bool TryBuyItemForAgent(int itemIndex)
        {
            if (_shoppingController == null) return false;
            return _shoppingController.TryBuyAgentItem(CurrentTeam, itemIndex);
        }

        public void TriggerGameEnd(TeamColor winningTeam)
        {
            bool isLocalPlayerWin = false;

            foreach (var slot in _activeSlots)
            {
                if (slot.TeamColor == winningTeam && slot.PlayerType == PlayerType.Human)
                {
                    isLocalPlayerWin = true;
                    break;
                }
            }

            // ML-Agent: Báo tín hiệu chia Reward Zero-Sum dựa trên thứ tự về đích thực tế
            boardController.OnGameRanked?.Invoke(_finishedTeams);

            if (isTrainingMode)
            {
                // Reload lại Scene hiện tại để tự động Reset toàn bộ biến số, Board, Vàng,... cho Tập huấn luyện mới
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if (gameEndView != null)
                gameEndView.Show(isLocalPlayerWin);
            else
                Debug.LogWarning("Chưa gán GameEndView vào TurnController!");
        }

        #endregion

        #region Bot Integration

        public void HandleBotTurn()
        {
            var currentSlot = _activeSlots[_currentTeamIndex];
            if (currentSlot.PlayerType == PlayerType.MLAgent)
            {
                // Gọi RequestDecision ủy quyền cho ML-Agent khi đến đúng lượt
                OnMLAgentTurn?.Invoke(CurrentTeam);
                return;
            }
            StartCoroutine(BotThinkingProcecss());
        }

        private IEnumerator BotThinkingProcecss()
        {
            yield return new WaitForSeconds(0.5f);

            //AI trang bị đồ
            var botSlot = _activeSlots[_currentTeamIndex];
            if (botSlot.Inventory != null && botSlot.Inventory.Items.Count > 0)
            {
                var usageDecisions =
                    _botDecisionService.GetItemUsageDecisions(CurrentTeam, botSlot.Inventory.Items,
                        boardController.Board);

                foreach (var decision in usageDecisions)
                {
                    bool success = _itemService.TryEquipItem(botSlot.Inventory, decision.TargetUnit, decision.Item);

                    if (success)
                    {
                        Debug.Log(
                            $"Bot {CurrentTeam} đã gắn {decision.Item.ItemName} cho Unit {decision.TargetUnit.Id}");
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            //AI quyết định di chuyển
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
                Debug.Log($"Bot {CurrentTeam} không có nước đi nào hợp lệ.");
                EndTurn();
            }
        }

        #endregion
    }
}