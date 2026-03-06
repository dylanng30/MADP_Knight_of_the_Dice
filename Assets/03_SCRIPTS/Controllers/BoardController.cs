using System;
using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Models.CellEvents;
using MADP.Models.UnitActions;
using MADP.Services;
using MADP.Services.CellEvent.Interfaces;
using MADP.Services.Combat.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Services.Pathfinding.Interfaces;
using MADP.Services.VFX.Interfaces;
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
        [SerializeField] private BoardView boardView;
        
        //Data
        private BoardModel _boardModel;
        public BoardModel Board => _boardModel;
        private Dictionary<TeamColor, List<UnitModel>> _allUnits;
        
        //Services
        private BoardModelGenerationService _boardModelGenerationService = new();
        private UnitModelGenerationService _unitModelGenerationService;
        
        public IPathfindingService PathfindingService { get; private set; }
        private IGoldService _goldService;
        private ICombatService _combatService;
        private ICellEventService _cellEventService;
        private IVFXService _vfxService;

        //Events
        public Action<BoardModel> OnBoardGenerated;
        public Action<Dictionary<TeamColor, List<UnitModel>>> OnAllUnitsGenerated;
        
        private List<LobbySlotModel> _activeSlots;
        private Dictionary<TeamColor, int> _teamToBaseMap = new();
        private MapType _currentMapType;
        
        private int _redCells;
        private int _yellowCells;
        private int _purpleCells;
        private int _greenCells;

        public void Initialize(
            IGoldService goldService, 
            IPathfindingService pathfindingService,
            ICombatService combatService,
            ICellEventService cellEventService, 
            IVFXService vfxService,
            int redCells,
            int yellowCells,
            int purpleCells,
            int greenCells,
            List<LobbySlotModel> activeSlots,
            MapType mapType,
            TeamColorDatabaseSO teamColorDB)
        {
            _unitModelGenerationService = new UnitModelGenerationService(teamStatDB);

            _goldService = goldService;
            PathfindingService = pathfindingService;
            _combatService = combatService;
            _cellEventService = cellEventService;
            _vfxService = vfxService;
            
            _activeSlots = activeSlots;
            _currentMapType = mapType;
            
            _redCells = redCells;
            _yellowCells = yellowCells;
            _purpleCells = purpleCells;
            _greenCells = greenCells;
            
            _teamToBaseMap.Clear();
            foreach (var slot in activeSlots)
            {
                _teamToBaseMap[slot.TeamColor] = slot.SlotIndex;
            }
            
            boardView.Initialize(this, _teamToBaseMap, _currentMapType, teamColorDB);
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
            if (!_boardModel.HomeCells.TryGetValue(teamColor, out var homeCells))
                return false;
            
            int unitsInWinningPosition = 0;
            
            foreach (var cell in homeCells)
            {
                if (cell.HasUnit && cell.Unit.TeamOwner == teamColor)
                {
                    int index = cell.Index;
                    if (index == 2 || index == 3 || index == 4 || index == 5)
                    {
                        unitsInWinningPosition++;
                    }
                }
            }

            return unitsInWinningPosition == 4;
        }
        

        #region --- MOVEMENT & COMBAT ---
        public void MoveUnit(UnitModel unitModel, CellModel targetCellModel, int diceValue, Action onMoveCompleted)
        {
            CellModel currentCellModel = GetCurrentCellOfUnit(unitModel);
            boardView.ClearAllHighlightsHints();

            if (targetCellModel.Structure == CellStructure.Home && targetCellModel.TeamOwner == unitModel.TeamOwner)
            {
                HandleMoveToHome(unitModel, currentCellModel, targetCellModel, diceValue, onMoveCompleted);
                return;
            }

            bool isSpecialGateJump = (diceValue == 1) &&
                                     (targetCellModel.Structure == CellStructure.Gate) &&
                                     !IsNextCell(currentCellModel, targetCellModel);
            
            bool isReverseKick = false;
            var reversePath = PathfindingService.GetReversePath(_boardModel, currentCellModel, diceValue);
            if (reversePath.Count > 0 && reversePath.Last() == targetCellModel)
            {
                isReverseKick = true;
                //Debug.LogError($"[ĐÁ HẬU] Unit {unitModel.Id} (Team {unitModel.TeamOwner}) đang thực hiện ĐÁ HẬU lùi về ô {targetCellModel.Index} có unit {targetCellModel.Unit.Id} của team {targetCellModel.Unit.TeamOwner}!");
            }
            
            List<CellModel> pathCells;
            if (isReverseKick)
            {
                pathCells = reversePath;
            }
            else if (isSpecialGateJump)
            {
                pathCells = PathfindingService.GetPathToGate(_boardModel, currentCellModel);
            }
            else
            {
                pathCells = PathfindingService.GetPath(_boardModel, currentCellModel, diceValue);
            }

            List<Vector3> fullVisualPath = boardView.GetPath(pathCells);

            if(IsCombatScenario(unitModel, targetCellModel))
            {
                HandleCombatMove(unitModel, currentCellModel, targetCellModel, fullVisualPath, onMoveCompleted);
            }
            else
            {
                HandleNormalMove(unitModel, currentCellModel, targetCellModel, fullVisualPath, onMoveCompleted);
            }

        }

        private void HandleMoveToHome(UnitModel unitModel, CellModel currentCellModel, CellModel targetCellModel, int diceValue, Action onMoveCompleted)
        {
            Debug.Log($"Unit {unitModel.Id} đã về tới chuồng của team {unitModel.TeamOwner}");
            List<CellModel> homePath = PathfindingService.GetPathToHome(_boardModel, currentCellModel, diceValue);
            List<Vector3> homeVisualPath = boardView.GetPath(homePath);

            ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);
            Vector3 forwarDirection = GetForwardDirection(targetCellModel);
            MoveUA moveToHomeUA = new MoveUA(boardView.GetUnitView(unitModel), homeVisualPath, forwarDirection);
            ActionSystem.Instance.Perform(moveToHomeUA, onMoveCompleted);
        }

        private void HandleCombatMove(UnitModel attacker, CellModel currentCell, CellModel targetCell, List<Vector3> fullVisualPath, Action onMoveCompleted)
        {
            UnitModel victim = targetCell.Unit;
            UnitView victimView = boardView.GetUnitView(victim);
            UnitView attackerView = boardView.GetUnitView(attacker);

            List<Vector3> approachPath = new List<Vector3>(fullVisualPath);

            if (approachPath.Count > 1)
                approachPath.RemoveAt(approachPath.Count - 1);

            MoveUA approachMoveUA = new MoveUA(attackerView, approachPath);

            CombatResult result = _combatService.SimulateCombat(attacker, victim);
            
            Action onHit = () => {
                victim.TakeDamage(result.DamageDealt);
            };
            
            Action onDeathAnimationFinished = () => {
                Debug.Log($"Unit {victim.Id} cua team {victim.TeamOwner} chết. Unit {attacker.Id} cua team {attacker.TeamOwner} chuẩn bị chiếm ô.");
                boardView.UnitReturnNest(victim);
                victim.Revive();
                victimView.Collider.enabled = true;
                victimView.PlayAnimation("Idle");
            };

            AttackUA attackUA = new AttackUA(attackerView, victimView, result.IsVictimDead, onHit, onDeathAnimationFinished);
            approachMoveUA.PostActions.Add(attackUA);

            if (result.IsVictimDead)
            {
                HandleCombatWin(attacker, victim, currentCell, targetCell, attackerView, attackUA, approachPath.Last(), fullVisualPath.Last(), result.DamageDealt);
            }
            else
            {
                HandleCombatFail(victim, attackerView, attackUA, approachPath, result.DamageDealt, currentCell);
            }
            ActionSystem.Instance.Perform(approachMoveUA, onMoveCompleted);
        }
        
        private void HandleCombatWin(
            UnitModel attacker, UnitModel victim, 
            CellModel currentCellModel, CellModel targetCellModel, 
            UnitView attackerUnitView, AttackUA attackUA, 
            Vector3 approachEndPos, Vector3 fullPathEndPos, int damage)
        {
            Debug.Log($"Unit {victim.Id} cua team {victim.TeamOwner} chết. Unit {attacker.Id} cua team {attacker.TeamOwner} chiếm ô.");
            targetCellModel.Clear();
            ExecuteMoveSuccess(attacker, currentCellModel, targetCellModel);
            
            var winStepPath = new List<Vector3> { approachEndPos, fullPathEndPos };
            Vector3 forwardDirection = GetForwardDirection(targetCellModel);
            MoveUA winMoveUA = new MoveUA(attackerUnitView, winStepPath, forwardDirection);
            
            attackUA.PostActions.Add(winMoveUA);
            TryAddCellEventAction(attacker, targetCellModel, winMoveUA);
        }

        private void HandleCombatFail(
            UnitModel victim, UnitView attackerView, 
            BaseUnitAction parentAction, List<Vector3> approachPath, int damage, CellModel currentCell)
        {
            var returnPath = new List<Vector3>(approachPath);
            returnPath.Reverse();
            
            Vector3 finalDirection = GetForwardDirection(currentCell);
            MoveUA returnMoveUA = new MoveUA(attackerView, returnPath, finalDirection);
            parentAction.PostActions.Add(returnMoveUA);
        }

        private void HandleNormalMove(UnitModel unitModel, CellModel currentCellModel, CellModel targetCellModel, List<Vector3> fullVisualPath, Action onMoveCompleted)
        {
            UnitView unitView = boardView.GetUnitView(unitModel);
            CellView targetCellView = boardView.GetCellView(targetCellModel);
            ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);
            unitView.transform.SetParent(targetCellView.transform);
            Vector3 forwardDirection = GetForwardDirection(targetCellModel);
            MoveUA moveUA = new MoveUA(unitView, fullVisualPath, forwardDirection);
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
            //Debug.Log($"Team {unitModel.TeamOwner} spawn unit {unitModel.Id}");
            CellModel spawnCell = _boardModel.AroundCells.FirstOrDefault(c => 
                c.Structure == CellStructure.Spawn && 
                c.TeamOwner == unitModel.TeamOwner &&
                !c.HasUnit);

            if (spawnCell == null)
            {
                Debug.LogError("Khong tim thay spawn cell");
                return;
            }

            if(_goldService.TrySpendGold(unitModel.TeamOwner, unitModel.Cost))
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
                    var direction = GetForwardDirection(spawnCell);
                    unitView.Rotate(direction);
                    unitView.Collider.enabled = false;
                }
                
                if (AudioController.Instance != null)
                {
                    AudioController.Instance.PlayUISound(SoundKey.SFX_BuySuccess);    
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
            bool isInvalidReverse = false;
            if (currentCell.Structure == CellStructure.Spawn && currentCell.TeamOwner == unit.TeamOwner)
            {
                isInvalidReverse = true;
            }
            else
            {
                for (int i = 1; i < reversePath.Count; i++)
                {
                    if (reversePath[i].Structure == CellStructure.Spawn && reversePath[i].TeamOwner == unit.TeamOwner)
                    {
                        isInvalidReverse = true;
                        break;
                    }
                }
            }
            if (!isInvalidReverse && reversePath.Count > 0 && !IsPathBlocked(reversePath, unit.TeamOwner))
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
            boardView.HighlightHints(cellModels);
        }
        public void HighlightSelection(CellModel cellModel)
        {
            boardView.HighlightSelection(cellModel);
        }

        public void ClearAllHighlights()
        {
            boardView.ClearAllHighlightsHints();
            boardView.ClearSelectionHighlight();
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
                if(CanInteract(unit, diceValue))
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
            if (unitModel.State != UnitState.InNest) 
                return false;

            var spawnCell = _boardModel.AroundCells.FirstOrDefault(
                c => c.Structure == CellStructure.Spawn && 
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
                if(pathModel[i].HasUnit) return true;
            }
            
            var lastCell = pathModel.Last();
            
            if(lastCell.HasUnit && 
               lastCell.Unit.TeamOwner == selfTeam)
                return true;
            
            return false;
        }

        private void ExecuteMoveSuccess(UnitModel unit, CellModel currentCell, CellModel targetCell)
        {
            currentCell.Clear();
            //Debug.Log($"Unit {unit.Id} của team {unit.TeamOwner} đi từ ô {currentCell.Index} đến {targetCell.Index}");
            targetCell.Register(unit);
            
            int distance = 0;
            if (targetCell.Structure == CellStructure.Home)
            {
                distance = 1; 
                unit.SetState(UnitState.InHome);
            }
            else
            {
                var forwardPath = PathfindingService.GetPath(_boardModel, currentCell, 64);
                int forwardDist = forwardPath.IndexOf(targetCell); 
        
                var revPath = PathfindingService.GetReversePath(_boardModel, currentCell, 64);
                int reverseDist = revPath.IndexOf(targetCell);

                if (forwardDist != -1 && (reverseDist == -1 || forwardDist <= reverseDist)) {
                    distance = forwardDist;
                } else if (reverseDist != -1) {
                    distance = -reverseDist;
                }
            }
            
            unit.AddSteps(distance);
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
                _redCells, 
                _yellowCells,
                _purpleCells,
                _greenCells,
                _activeSlots
            );

            OnBoardGenerated?.Invoke(_boardModel);
        }
        private void GenerateUnits()
        {
            _allUnits = _unitModelGenerationService.CreateAllUnits(_activeSlots); 
            OnAllUnitsGenerated?.Invoke(_allUnits);
        }
        #endregion
        
        #region ---HELPER---
        public CellModel GetSpawnCell(TeamColor teamColor)
        {
            return _boardModel?.AroundCells.FirstOrDefault(c => c.Structure == CellStructure.Spawn && c.TeamOwner == teamColor);
        }
        public bool IsOvershootingGate(UnitModel unit, int diceValue)
        {
            if (unit.State != UnitState.Moving) return false;

            CellModel currentCell = GetCurrentCellOfUnit(unit);
            if (currentCell.Structure == CellStructure.Gate && currentCell.TeamOwner == unit.TeamOwner)
                return false;

            var path = PathfindingService.GetPath(_boardModel, currentCell, diceValue);

            for (int i = 1; i < path.Count; i++)
            {
                CellModel cell = path[i];
                if (cell.Structure == CellStructure.Gate && cell.TeamOwner == unit.TeamOwner)
                {
                    if (i < path.Count - 1)
                    {
                        //Debug.LogError($"Unit {unit.Id} của team {unit.TeamOwner} có thể đi qua với xúc xắc {diceValue}");
                        return true;
                    }
                }
            }

            return false;
        }
        private Vector3 GetForwardDirection(CellModel cell)
        {
            if (cell.Structure == CellStructure.Home)
            {
                var homeCells = _boardModel.HomeCells[cell.TeamOwner];
                int currentIndex = homeCells.IndexOf(cell);
                if (currentIndex >= 0 && currentIndex < homeCells.Count - 1)
                {
                    var currentView = boardView.GetCellView(cell);
                    var nextView = boardView.GetCellView(homeCells[currentIndex + 1]);
                    return nextView.transform.position - currentView.transform.position;
                }
            }
            else
            {
                int nextCellIndex = (cell.Index + 1) % _boardModel.AroundCells.Count;
                var nextCell = _boardModel.AroundCells[nextCellIndex];
                if (nextCell != null)
                {
                    var currentView = boardView.GetCellView(cell);
                    var nextView = boardView.GetCellView(nextCell);
                    return nextView.transform.position - currentView.transform.position;
                }
            }
            return Vector3.zero;
        }
        #endregion

    }
}