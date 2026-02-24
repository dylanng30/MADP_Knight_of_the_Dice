using System;
using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Models.UnitActions;
using MADP.Services;
using MADP.Services.CellEvent.Interfaces;
using MADP.Services.Combat.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Services.Pathfinding.Interfaces;
using MADP.Settings;
using MADP.Systems;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Controllers
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] private TeamStatDatabaseSO teamStatDB;
        [SerializeField] private BoardSetting boardSetting;
        [SerializeField] private BoardView boardView;

        //Data
        private BoardModel _boardModel;
        public BoardModel Board => _boardModel;
        private Dictionary<TeamColor, List<UnitModel>> _allUnits;

        //Services
        private BoardModelGenerationService _boardModelGenerationService = new();
        private UnitModelGenerationService _unitModelGenerationService = new();

        public IPathfindingService PathfindingService { get; private set; }
        private IGoldService _goldService;
        private ICombatService _combatService;
        private ICellEventService _cellEventService;

        //Events
        public Action<BoardModel> OnBoardGenerated;
        public Action<Dictionary<TeamColor, List<UnitModel>>> OnAllUnitsGenerated;

        private List<LobbySlotModel> _activeSlots;
        private Dictionary<TeamColor, int> _teamToBaseMap = new();
        private MapType _currentMapType;

        public void Initialize(
            IGoldService goldService,
            IPathfindingService pathfindingService,
            ICombatService combatService,
            ICellEventService cellEventService,
            List<LobbySlotModel> activeSlots,
            MapType mapType)
        {

            _unitModelGenerationService = new UnitModelGenerationService(teamStatDB);


            _goldService = goldService;
            PathfindingService = pathfindingService;
            _combatService = combatService;
            _cellEventService = cellEventService;
            _activeSlots = activeSlots;
            _currentMapType = mapType;

            _teamToBaseMap.Clear();
            foreach (var slot in activeSlots)
            {
                _teamToBaseMap[slot.TeamColor] = slot.SlotIndex;
            }
        }

        private void Start()
        {
            boardView.Initialize(this, _teamToBaseMap, _currentMapType);
            StartGame();
        }

        public void StartGame()
        {
            boardView.Reset();
            GenerateBoard();
            GenerateUnits();
        }

        public bool CheckWinCondition(TeamColor teamColor)
        {
            var homeCells = _boardModel.HomeCells[teamColor];
            int rightCell = 0;
            foreach (var cell in homeCells)
            {
                int index = cell.Index;
                if((index == 6 || index == 5 || index == 4 || index == 3) && 
                   cell.HasUnit)
                {
                    rightCell++;
                }
            }
            return rightCell == 4;
        }
        

        #region --- MOVEMENT & COMBAT ---

        public void MoveUnit(UnitModel unitModel, CellModel targetCellModel, int diceValue, Action onMoveCompleted)
        {
            CellModel currentCellModel = GetCurrentCellOfUnit(unitModel);
            boardView.ClearAllHighlights();

            if (targetCellModel.Structure == CellStructure.Home && targetCellModel.TeamOwner == unitModel.TeamOwner)
            {
                HandleMoveToHome(unitModel, currentCellModel, targetCellModel, diceValue, onMoveCompleted);
                return;
            }

            bool isSpecialGateJump = (diceValue == 1) &&
                                     (targetCellModel.Structure == CellStructure.Gate) &&
                                     !IsNextCell(currentCellModel, targetCellModel);

            List<CellModel> pathCells = isSpecialGateJump
                ? PathfindingService.GetPathToGate(_boardModel, currentCellModel)
                : PathfindingService.GetPath(_boardModel, currentCellModel, diceValue);

            List<Vector3> fullVisualPath = boardView.GetPath(pathCells);

            if (IsCombatScenario(unitModel, targetCellModel))
            {
                HandleCombatMove(unitModel, currentCellModel, targetCellModel, fullVisualPath, onMoveCompleted);
            }
            else
            {
                HandleNormalMove(unitModel, currentCellModel, targetCellModel, fullVisualPath, onMoveCompleted);
            }
        }

        private void HandleMoveToHome(UnitModel unitModel, CellModel currentCellModel, CellModel targetCellModel,
            int diceValue, Action onMoveCompleted)
        {
            Debug.Log($"Unit {unitModel.Id} đã về tới chuồng của team {unitModel.TeamOwner}");
            List<CellModel> homePath = PathfindingService.GetPathToHome(_boardModel, currentCellModel, diceValue);
            List<Vector3> homeVisualPath = boardView.GetPath(homePath);

            ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);
            MoveUA moveToHomeUA = new MoveUA(boardView.GetUnitView(unitModel), homeVisualPath);
            ActionSystem.Instance.Perform(moveToHomeUA, onMoveCompleted);
        }

        private void HandleCombatMove(UnitModel attacker, CellModel currentCell, CellModel targetCell,
            List<Vector3> fullVisualPath, Action onMoveCompleted)
        {
            UnitModel victim = targetCell.Unit;
            UnitView victimView = boardView.GetUnitView(victim);
            UnitView attackerView = boardView.GetUnitView(attacker);

            List<Vector3> approachPath = new List<Vector3>(fullVisualPath);

            if (approachPath.Count > 1)
                approachPath.RemoveAt(approachPath.Count - 1);

            MoveUA approachMoveUA = new MoveUA(attackerView, approachPath);

            CombatResult result = _combatService.SimulateCombat(attacker, victim);

            AttackUA attackUA = new AttackUA(attackerView, victimView, result.IsVictimDead);
            approachMoveUA.PostActions.Add(attackUA);

            if (result.IsVictimDead)
            {
                HandleCombatWin(attacker, victim, currentCell, targetCell, attackerView, attackUA, approachPath.Last(),
                    fullVisualPath.Last(), result.DamageDealt);
            }
            else
            {
                HandleCombatFail(victim, attackerView, attackUA, approachPath, result.DamageDealt);
            }

            ActionSystem.Instance.Perform(approachMoveUA, onMoveCompleted);
        }

        private void HandleCombatWin(
            UnitModel attacker, UnitModel victim,
            CellModel currentCellModel, CellModel targetCellModel,
            UnitView attackerUnitView, AttackUA attackUA,
            Vector3 approachEndPos, Vector3 fullPathEndPos, int damageDealt)
        {
            Debug.Log($"Unit {victim.Id} chết. Unit {attacker.Id} chiếm ô.");
            victim.TakeDamage(damageDealt);
            boardView.UnitReturnNest(victim);
            victim.Revive();
            ExecuteMoveSuccess(attacker, currentCellModel, targetCellModel);
            CellView targetCellView = boardView.GetCellView(targetCellModel);
            attackerUnitView.transform.SetParent(targetCellView.transform);
            var winStepPath = new List<Vector3> { approachEndPos, fullPathEndPos };
            MoveUA winMoveUA = new MoveUA(attackerUnitView, winStepPath);
            attackUA.PostActions.Add(winMoveUA);
            TryAddCellEventAction(attacker, targetCellModel, winMoveUA);
        }

        private void HandleCombatFail(
            UnitModel victim, UnitView attackerView,
            BaseUnitAction parentAction, List<Vector3> approachPath, int damage)
        {
            victim.TakeDamage(damage);
            var returnPath = new List<Vector3>(approachPath);
            returnPath.Reverse();
            parentAction.PostActions.Add(new MoveUA(attackerView, returnPath));
        }

        private void HandleNormalMove(UnitModel unitModel, CellModel currentCellModel, CellModel targetCellModel,
            List<Vector3> fullVisualPath, Action onMoveCompleted)
        {
            UnitView unitView = boardView.GetUnitView(unitModel);
            CellView targetCellView = boardView.GetCellView(targetCellModel);
            ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);
            unitView.transform.SetParent(targetCellView.transform);
            MoveUA moveUA = new MoveUA(unitView, fullVisualPath);
            TryAddCellEventAction(unitModel, targetCellModel, moveUA);
            ActionSystem.Instance.Perform(moveUA, onMoveCompleted);
        }

        private bool IsCombatScenario(UnitModel unit, CellModel target)
        {
            return target.HasUnit && target.Unit.TeamOwner != unit.TeamOwner;
        }

        private void TryAddCellEventAction(UnitModel unitModel, CellModel cellModel, BaseUnitAction parentAction)
        {
            var cellEvent = _cellEventService.GetEvent(cellModel);

            if (cellEvent != null)
            {
                var unitView = boardView.GetUnitView(unitModel);
                var cellView = boardView.GetCellView(cellModel);
                var eventAction = new CellEventUA(unitModel, unitView, cellModel, cellView, cellEvent);
                parentAction.PostActions.Add(eventAction);
            }
        }

        #endregion

        #region --- GAMEPLAY LOGIC ---

        public void SpawnUnit(UnitModel unitModel, Action OnComplete)
        {
            CellModel spawnCell = _boardModel.AroundCells.FirstOrDefault(c =>
                c.Structure == CellStructure.Spawn &&
                c.TeamOwner == unitModel.TeamOwner &&
                !c.HasUnit);

            if (spawnCell == null) return;

            if (_goldService.TrySpendGold(unitModel.TeamOwner, unitModel.Cost))
            {
                spawnCell.Register(unitModel);
                unitModel.SetState(UnitState.Moving);

                UnitView unitView = boardView.GetUnitView(unitModel);
                CellView spawnCellView = boardView.GetCellView(spawnCell);

                if (unitView != null)
                {
                    unitView.transform.SetParent(spawnCellView.transform);
                    Vector3 targetPos = boardView.GetCellPosition(spawnCell);
                    unitView.Spawn(targetPos);
                    var direction = GetDirectionWhenSpawn(unitModel);
                    unitView.Rotate(direction);
                    unitView.Collider.enabled = false;
                }

                OnComplete?.Invoke();
            }
            else
            {
                Debug.Log("Not enough gold to spawn unit");

                //Show UI thông báo không đủ vàng
            }
        }

        public List<CellModel> GetPotentialDestinationCell(UnitModel unitModel, int diceValue)
        {
            List<CellModel> potentialCells = new List<CellModel>();

            switch (unitModel.State)
            {
                case UnitState.InHome:
                    GetHomeMoveOptions(unitModel, diceValue, potentialCells);
                    break;
                case UnitState.Moving:
                    GetMovingOptions(unitModel, diceValue, potentialCells);
                    break;
            }

            return potentialCells;
        }

        //Xử lý khi Unit đang ở trong Home
        private void GetHomeMoveOptions(UnitModel unit, int diceValue, List<CellModel> results)
        {
            if (!_boardModel.HomeCells.TryGetValue(unit.TeamOwner, out var homeCells)) return;

            var currentCell = homeCells.FirstOrDefault(c => c.Unit == unit);
            if (currentCell == null) return;

            int nextIndex = currentCell.Index + 1; //Tính index của ô tiếp theo
            if (nextIndex >= homeCells.Count) return;

            if ((nextIndex + 1) == diceValue) //Kiểm tra thứ tự của ô tiếp theo có đúng với giá trị xúc xắc không
            {
                var targetCell = homeCells[nextIndex];

                if (!targetCell.HasUnit) //Kiểm tra ô đích có trống không
                {
                    results.Add(targetCell);
                }
            }
        }

        //Xử lý khi Unit đang di chuyển ở vòng ngoài
        private void GetMovingOptions(UnitModel unit, int diceValue, List<CellModel> results)
        {
            var currentCell = GetCurrentCellOfUnit(unit);
            if (currentCell == null) return;

            //Unit đang ở Gate
            if (currentCell.Structure == CellStructure.Gate && 
                currentCell.TeamOwner == unit.TeamOwner)
            {
                GetGateEntryOptions(unit, diceValue, results);
            }
            else //Di chuyển trên đường
            {
                GetNormalMoveOptions(unit, currentCell, diceValue, results);
            }
        }

        //Xử lý khi đi vào các ô Home
        private void GetGateEntryOptions(UnitModel unit, int diceValue, List<CellModel> results)
        {
            if (!_boardModel.HomeCells.TryGetValue(unit.TeamOwner, out var homeCells)) return;

            //Kiểm tra đường đi trong nhà có bị chặn không
            for (int i = 0; i < diceValue; i++)
            {
                if (i >= homeCells.Count) break;

                var cell = homeCells[i];
                bool isDestination = (i == diceValue - 1);

                if (isDestination)
                {
                    if (!cell.HasUnit)
                        results.Add(cell);
                }
                else
                {
                    //Bị chặn, không đi được
                    if (cell.HasUnit) return;
                }
            }
        }

        //Xử lý di chuyển ở các ô thường
        private void GetNormalMoveOptions(UnitModel unit, CellModel currentCell, int diceValue, List<CellModel> results)
        {
            //Nhảy cóc
            if (diceValue == 1)
            {
                var pathToGate = PathfindingService.GetPathToGate(_boardModel, currentCell);
                if (pathToGate.Count > 0 && !IsPathBlocked(pathToGate, unit.TeamOwner))
                {
                    results.Add(pathToGate.Last());
                    return;
                }
            }

            //Đá tiến
            var forwardPath = PathfindingService.GetPath(_boardModel, currentCell, diceValue);
            if (forwardPath.Count > 0 && !IsPathBlocked(forwardPath, unit.TeamOwner))
            {
                results.Add(forwardPath.Last());
            }

            //Đá hậu
            var reversePath = PathfindingService.GetReversePath(_boardModel, currentCell, diceValue);
            if (reversePath.Count > 0 && !IsPathBlocked(reversePath, unit.TeamOwner))
            {
                var targetCell = reversePath.Last();
                //Chỉ được đá nếu ô đích CÓ ĐỊCH
                if (targetCell.HasUnit && targetCell.Unit.TeamOwner != unit.TeamOwner)
                {
                    results.Add(targetCell);
                }
            }
        }

        private bool IsNextCell(CellModel cellA, CellModel cellB)
        {
            var list = _boardModel.AroundCells;
            int indexA = list.IndexOf(cellA);
            int indexNext = (indexA + 1) % list.Count;
            return list[indexNext] == cellB;
        }

        public void HighlightCells(List<CellModel> cellModels)
        {
            boardView.HighlightCells(cellModels);
        }

        public void ClearAllHighlights()
        {
            boardView.ClearAllHighlights();
        }

        public List<UnitModel> GetAllUnitsByColor(TeamColor teamColor)
        {
            return _allUnits[teamColor];
        }

        public bool CheckIfAnyMovePossible(TeamColor team, int diceValue)
        {
            List<UnitModel> units = GetAllUnitsByColor(team);

            foreach (UnitModel unit in units)
            {
                if (CanInteract(unit, diceValue))
                    return true;
            }

            return false;
        }

        public bool CanInteract(UnitModel unitModel, int diceValue)
        {
            return CanSpawnUnit(unitModel, diceValue) ||
                   CanMoveUnit(unitModel, diceValue);
        }

        public bool CanSpawnUnit(UnitModel unitModel, int diceValue)
        {
            if (unitModel.State == UnitState.Moving)
                return false;

            var spawnCell = _boardModel.AroundCells.FirstOrDefault(c => c.Structure == CellStructure.Spawn &&
                                                                        c.TeamOwner == unitModel.TeamOwner);

            return diceValue == 6 && !spawnCell.HasUnit &&
                   _goldService.GetGold(unitModel.TeamOwner) >= unitModel.Cost;
        }

        public bool CanMoveUnit(UnitModel unitModel, int diceValue)
        {
            if (unitModel.State == UnitState.InNest)
                return false;

            return GetPotentialDestinationCell(unitModel, diceValue).Count > 0;
        }

        private bool IsPathBlocked(List<CellModel> pathModel, TeamColor selfTeam)
        {
            if (pathModel == null || pathModel.Count == 0)
                return false;

            for (int i = 1; i < pathModel.Count - 1; i++)
            {
                if (pathModel[i].HasUnit) return true;
            }

            var lastCell = pathModel.Last();

            if (lastCell.HasUnit &&
                lastCell.Unit.TeamOwner == selfTeam)
                return true;

            return false;
        }

        private void ExecuteMoveSuccess(UnitModel unit, CellModel currentCell, CellModel targetCell)
        {
            if (currentCell != null)
            {
                currentCell.Clear();
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy ô cũ của Unit {unit.Id}");
            }

            targetCell.Register(unit);

            var distance = Mathf.Abs(targetCell.Index - currentCell.Index);
            unit.AddSteps(distance);

            if (targetCell.Structure == CellStructure.Home)
            {
                unit.SetState(UnitState.InHome);
            }
        }

        public CellModel GetCurrentCellOfUnit(UnitModel unit)
        {
            //Trong aroundCells 
            var cell = _boardModel.AroundCells.FirstOrDefault(c => c.Unit == unit);
            if (cell != null) return cell;

            //Trong homeCells
            if (_boardModel.HomeCells.TryGetValue(unit.TeamOwner, out var homeCells))
            {
                return homeCells.FirstOrDefault(c => c.Unit == unit);
            }

            //Trong nest
            return null;
        }

        #endregion

        #region ---INITIALIZE---

        private void GenerateBoard()
        {
            _boardModel = _boardModelGenerationService.CreateFullBoard(
                boardSetting.RedCellCount,
                boardSetting.YellowCellCount,
                boardSetting.PurpleCellCount,
                _activeSlots
            );

            OnBoardGenerated?.Invoke(_boardModel);
        }

        private void GenerateUnits()
        {
            List<TeamColor> activeTeams = _activeSlots.Select(s => s.TeamColor).ToList();
            _allUnits = _unitModelGenerationService.CreateAllUnits(activeTeams); 
            OnAllUnitsGenerated?.Invoke(_allUnits);
        }

        #endregion

        #region ---HELPER---

        private Vector3 GetDirectionWhenSpawn(UnitModel unit)
        {
            var spawnCell = _boardModel.AroundCells.FirstOrDefault(c => c.Unit == unit);
            int nextCellIndex = (spawnCell.Index + 1) % _boardModel.AroundCells.Count;
            var nextCell = _boardModel.AroundCells[nextCellIndex];
            if (nextCell != null && spawnCell != null)
            {
                var spawnView = boardView.GetCellView(spawnCell);
                var nextView = boardView.GetCellView(nextCell);
                Vector3 direction = nextView.transform.position - spawnView.transform.position;
                return direction;
            }

            return Vector3.zero;
        }

        public CellModel GetSpawnCell(TeamColor teamColor)
        {
            return _boardModel?.AroundCells.FirstOrDefault(c =>
                c.Structure == CellStructure.Spawn && c.TeamOwner == teamColor);
        }

        #endregion

        #region ---MLAgent---

        public void MoveOnNormalCellOnly(UnitModel unitModel, int diceValue, Action onMoveCompleted)
        {
            CellModel currentCell = GetCurrentCellOfUnit(unitModel);
            if (currentCell == null) return;

            // Chỉ cho phép ở vòng ngoài
            if (!_boardModel.AroundCells.Contains(currentCell)) return;

            // Lấy path tiến
            List<CellModel> path = PathfindingService.GetPath(_boardModel, currentCell, diceValue);

            if (path == null || path.Count == 0) return;

            // Không cho đi xuyên unit
            if (IsPathBlocked(path, unitModel.TeamOwner)) return;

            CellModel targetCell = path.Last();

            // Không cho đứng chồng lên đồng đội
            if (targetCell.HasUnit) return;

            // === EXECUTE MOVE ===
            UnitView unitView = boardView.GetUnitView(unitModel);
            CellView targetCellView = boardView.GetCellView(targetCell);

            ExecuteMoveSuccess(unitModel, currentCell, targetCell);

            unitView.transform.SetParent(targetCellView.transform);

            List<Vector3> visualPath = boardView.GetPath(path);
            MoveUA moveUA = new MoveUA(unitView, visualPath);

            ActionSystem.Instance.Perform(moveUA, onMoveCompleted);
        }

        public void AttackForwardOnly(UnitModel attacker, int diceValue, Action onCompleted)
        {
            // Lấy ô hiện tại
            CellModel currentCell = GetCurrentCellOfUnit(attacker);
            if (currentCell == null) return;

            // Phải đang ở vòng ngoài
            if (!_boardModel.AroundCells.Contains(currentCell)) return;

            // Lấy path tiến
            List<CellModel> path = PathfindingService.GetPath(_boardModel, currentCell, diceValue);
            if (path == null || path.Count == 0) return;

            // Không được bị chặn giữa đường
            for (int i = 1; i < path.Count - 1; i++)
            {
                if (path[i].HasUnit) return;
            }

            CellModel targetCell = path.Last();

            // Ô cuối phải có địch
            if (!targetCell.HasUnit || targetCell.Unit.TeamOwner == attacker.TeamOwner) return;

            UnitModel victim = targetCell.Unit;

            UnitView attackerView = boardView.GetUnitView(attacker);
            UnitView victimView = boardView.GetUnitView(victim);

            List<Vector3> visualPath = boardView.GetPath(path);

            // Simulate combat
            CombatResult result = _combatService.SimulateCombat(attacker, victim);

            MoveUA moveUA = new MoveUA(attackerView, visualPath);
            AttackUA attackUA = new AttackUA(attackerView, victimView, result.IsVictimDead);

            moveUA.PostActions.Add(attackUA);

            if (result.IsVictimDead)
            {
                // Cập nhật model
                victim.TakeDamage(result.DamageDealt);
                boardView.UnitReturnNest(victim);
                victim.Revive();

                currentCell.Clear();
                targetCell.Register(attacker);

                int count = _boardModel.AroundCells.Count;
                int distance = (targetCell.Index - currentCell.Index + count) % count;
                attacker.AddSteps(distance);

                attackerView.transform.SetParent(boardView.GetCellView(targetCell).transform);
            }
            else
            {
                victim.TakeDamage(result.DamageDealt);
            }

            ActionSystem.Instance.Perform(moveUA, onCompleted);
        }

        public void AttackBackwardOnly(UnitModel attacker, int diceValue, Action onCompleted)
        {
            // Lấy ô hiện tại
            CellModel currentCell = GetCurrentCellOfUnit(attacker);
            if (currentCell == null) return;

            // Phải ở vòng ngoài
            if (!_boardModel.AroundCells.Contains(currentCell)) return;

            // Lấy path lùi
            List<CellModel> reversePath = PathfindingService.GetReversePath(_boardModel, currentCell, diceValue);
            if (reversePath == null || reversePath.Count == 0) return;

            // Không được bị chặn giữa đường
            for (int i = 1; i < reversePath.Count - 1; i++)
            {
                if (reversePath[i].HasUnit) return;
            }

            CellModel targetCell = reversePath.Last();

            // Ô cuối phải có địch
            if (!targetCell.HasUnit || targetCell.Unit.TeamOwner == attacker.TeamOwner) return;

            UnitModel victim = targetCell.Unit;

            UnitView attackerView = boardView.GetUnitView(attacker);
            UnitView victimView = boardView.GetUnitView(victim);

            List<Vector3> visualPath = boardView.GetPath(reversePath);

            // Combat
            CombatResult result = _combatService.SimulateCombat(attacker, victim);

            MoveUA moveUA = new MoveUA(attackerView, visualPath);
            AttackUA attackUA = new AttackUA(attackerView, victimView, result.IsVictimDead);

            moveUA.PostActions.Add(attackUA);

            if (result.IsVictimDead)
            {
                victim.TakeDamage(result.DamageDealt);
                boardView.UnitReturnNest(victim);
                victim.Revive();

                // Update model
                currentCell.Clear();
                targetCell.Register(attacker);

                int count = _boardModel.AroundCells.Count;
                int distance = (currentCell.Index - targetCell.Index + count) % count;
                attacker.AddSteps(distance);

                attackerView.transform.SetParent(boardView.GetCellView(targetCell).transform);
            }
            else
            {
                victim.TakeDamage(result.DamageDealt);
            }

            ActionSystem.Instance.Perform(moveUA, onCompleted);
        }

        public void MoveToHomeOnly(UnitModel unitModel, int diceValue, Action onCompleted)
        {
            // Lấy ô hiện tại
            CellModel currentCell = GetCurrentCellOfUnit(unitModel);
            if (currentCell == null) return;

            // Phải đang ở Gate của team mình
            if (currentCell.Structure != CellStructure.Gate || currentCell.TeamOwner != unitModel.TeamOwner) return;

            // Lấy path vào home
            List<CellModel> homePath = PathfindingService.GetPathToHome(_boardModel, currentCell, diceValue);
            if (homePath == null || homePath.Count == 0) return;

            // Không được bị chặn giữa đường
            for (int i = 0; i < homePath.Count; i++)
            {
                if (homePath[i].HasUnit) return;
            }

            CellModel targetCell = homePath.Last();

            // Execute Move
            UnitView unitView = boardView.GetUnitView(unitModel);
            CellView targetCellView = boardView.GetCellView(targetCell);

            // Clear ô Gate
            currentCell.Clear();

            // Register vào home
            targetCell.Register(unitModel);
            unitModel.SetState(UnitState.InHome);

            // Tính step (nếu bạn cần tracking)
            unitModel.AddSteps(diceValue);
            unitView.transform.SetParent(targetCellView.transform);

            List<Vector3> visualPath = boardView.GetPath(homePath);
            MoveUA moveUA = new MoveUA(unitView, visualPath);

            ActionSystem.Instance.Perform(moveUA, onCompleted);
        }

        public void MoveInsideHomeOnly(UnitModel unitModel, int diceValue, Action onCompleted)
        {
            if (!_boardModel.HomeCells.TryGetValue(unitModel.TeamOwner, out var homeCells)) return;

            CellModel currentCell = GetCurrentCellOfUnit(unitModel);
            if (currentCell == null) return;

            if (currentCell.Structure != CellStructure.Home) return;

            int currentIndex = homeCells.IndexOf(currentCell);
            if (currentIndex < 0) return;

            int nextIndex = currentIndex + 1;

            // Không có ô tiếp theo
            if (nextIndex >= homeCells.Count) return;

            // Điều kiện quan trọng: dice phải đúng số ô tiếp theo
            if (diceValue != nextIndex + 1) return;

            CellModel targetCell = homeCells[nextIndex];

            if (targetCell.HasUnit) return;

            // ===== Execute Move =====

            UnitView unitView = boardView.GetUnitView(unitModel);
            CellView targetCellView = boardView.GetCellView(targetCell);

            currentCell.Clear();
            targetCell.Register(unitModel);

            unitModel.AddSteps(1); // chỉ đi 1 ô

            unitView.transform.SetParent(targetCellView.transform);

            List<Vector3> visualPath = boardView.GetPath(new List<CellModel> { targetCell });

            MoveUA moveUA = new MoveUA(unitView, visualPath);

            ActionSystem.Instance.Perform(moveUA, onCompleted);
        }

        #endregion

    }
}