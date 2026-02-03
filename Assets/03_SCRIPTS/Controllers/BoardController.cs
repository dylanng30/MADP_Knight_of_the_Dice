using System;
using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Models.UnitActions;
using MADP.Services;
using MADP.Services.Combat.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Services.Pathfinding.Interfaces;
using MADP.Settings;
using MADP.Systems;
using MADP.Views;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace MADP.Controllers
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] private BoardSetting _boardSetting;
        [SerializeField] private BoardView _boardView;
        
        //Data
        private BoardModel _boardModel;
        private Dictionary<TeamColor, List<UnitModel>> _allUnits;
        
        //Services
        private BoardModelGenerationService _boardModelGenerationService = new();
        private UnitModelGenerationService _unitModelGenerationService = new();
        
        private IPathfindingService _pathfindingService;
        private IGoldService _goldService;
        private ICombatService _combatService;

        //Events
        public Action<BoardModel> OnBoardGenerated;
        public Action<Dictionary<TeamColor, List<UnitModel>>> OnAllUnitsGenerated;

        public void Initialize(
            IGoldService goldService, 
            IPathfindingService pathfindingService,
            ICombatService combatService)
        {
            _goldService = goldService;
            _pathfindingService = pathfindingService;
            _combatService = combatService;
        }

        private void Start()
        {
            _boardView.Initialize(this);
            StartGame();
        }

        public void StartGame()
        {
            _boardView.Reset();
            GenerateBoard();
            GenerateUnits();
        }

        #region --- MOVEMENT & COMBAT ---
        public void MoveUnit(UnitModel unitModel, CellModel targetCellModel, int diceValue, Action OnMoveCompleted)
        {
            CellModel currentCellModel = GetCurrentCellOfUnit(unitModel);
            _boardView.ClearAllHighlights();

            if (targetCellModel.Structure == CellStructure.Home && targetCellModel.TeamOwner == unitModel.TeamOwner)
            {
                HandleMoveToHome(unitModel, currentCellModel, targetCellModel, diceValue, OnMoveCompleted);
                return;
            }

            bool isSpecialGateJump = (diceValue == 1) &&
                                     (targetCellModel.Structure == CellStructure.Gate) &&
                                     !IsNextCell(currentCellModel, targetCellModel);

            List<CellModel> pathCells = isSpecialGateJump ?
                _pathfindingService.GetPathToGate(_boardModel, currentCellModel) :
                _pathfindingService.GetPath(_boardModel, currentCellModel, diceValue);

            List<Vector3> fullVisualPath = _boardView.GetPath(pathCells);

            if(IsCombatScenario(unitModel, targetCellModel))
            {
                HandleCombatMove(unitModel, currentCellModel, targetCellModel, fullVisualPath, OnMoveCompleted);
            }
            else
            {
                HandleNormalMove(unitModel, currentCellModel, targetCellModel, fullVisualPath, OnMoveCompleted);
            }

        }

        private void HandleMoveToHome(UnitModel unitModel, CellModel currentCellModel, CellModel targetCellModel, int diceValue, Action OnMoveCompleted)
        {
            Debug.Log($"Unit {unitModel.Id} đã về tới chuồng của team {unitModel.TeamOwner}");
            List<CellModel> homePath = _pathfindingService.GetPathToHome(_boardModel, currentCellModel, diceValue);
            List<Vector3> homeVisualPath = _boardView.GetPath(homePath);

            ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);
            MoveUA moveToHomeUA = new MoveUA(_boardView.GetUnitView(unitModel), homeVisualPath);
            ActionSystem.Instance.Perform(moveToHomeUA, OnMoveCompleted);
        }

        private void HandleCombatMove(UnitModel attacker, CellModel currentCell, CellModel targetCell, List<Vector3> fullVisualPath, Action OnMoveCompleted)
        {
            UnitModel victim = targetCell.Unit;
            UnitView victimView = _boardView.GetUnitView(victim);
            UnitView attackerView = _boardView.GetUnitView(attacker);

            List<Vector3> approachPath = new List<Vector3>(fullVisualPath);

            if (approachPath.Count > 1)
                approachPath.RemoveAt(approachPath.Count - 1);

            MoveUA approachMoveUA = new MoveUA(attackerView, approachPath);

            CombatResult result = _combatService.SimulateCombat(attacker, victim);

            AttackUA attackUA = new AttackUA(attackerView, victimView, result.IsVictimDead);
            approachMoveUA.PostActions.Add(attackUA);

            if (result.IsVictimDead)
            {
                HandleCombatWin(attacker, victim, currentCell, targetCell, attackerView, attackUA, approachPath.Last(), fullVisualPath.Last(), result.DamageDealt);
            }
            else
            {
                HandleCombatFail(victim, attackerView, attackUA, approachPath, result.DamageDealt);
            }

            ActionSystem.Instance.Perform(approachMoveUA, OnMoveCompleted);
        }
        
        private void HandleCombatWin(
            UnitModel attacker, UnitModel victim, 
            CellModel currentCellModel, CellModel targetCellModel, 
            UnitView attackerUnitView, AttackUA attackUA, 
            Vector3 approachEndPos, Vector3 fullPathEndPos, int damageDealt)
        {
            Debug.Log($"Unit {victim.Id} chết. Unit {attacker.Id} chiếm ô.");
            victim.TakeDamage(damageDealt);
            _boardView.UnitReturnNest(victim);
            victim.Revive();
            ExecuteMoveSuccess(attacker, currentCellModel, targetCellModel);
            CellView targetCellView = _boardView.GetCellView(targetCellModel);
            attackerUnitView.transform.SetParent(targetCellView.transform);
            var winStepPath = new List<Vector3> { approachEndPos, fullPathEndPos };
            MoveUA winMoveUA = new MoveUA(attackerUnitView, winStepPath);
            attackUA.PostActions.Add(winMoveUA);
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

        private void HandleNormalMove(UnitModel unitModel, CellModel currentCellModel, CellModel targetCellModel, List<Vector3> fullVisualPath, Action OnMoveCompleted)
        {
            UnitView unitView = _boardView.GetUnitView(unitModel);
            CellView targetCellView = _boardView.GetCellView(targetCellModel);
            ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);
            unitView.transform.SetParent(targetCellView.transform);
            MoveUA moveUA = new MoveUA(unitView, fullVisualPath);
            ActionSystem.Instance.Perform(moveUA, OnMoveCompleted);
        }

        private bool IsCombatScenario(UnitModel unit, CellModel target)
        {
            return target.HasUnit && target.Unit.TeamOwner != unit.TeamOwner;
        }
        #endregion

        #region --- GAMEPLAY LOGIC ---

        public void SpawnUnit(UnitModel unitModel, Action OnComplete)
        {
            CellModel spawnCell = _boardModel.AroundCells.FirstOrDefault(c => 
                c.Structure == CellStructure.Gate && 
                c.TeamOwner == unitModel.TeamOwner &&
                !c.HasUnit);
            
            if (spawnCell == null) return;

            if(_goldService.TrySpendGold(unitModel.TeamOwner, unitModel.Cost))
            {
                spawnCell.Register(unitModel);
                unitModel.SetState(UnitState.Moving);
                
                UnitView unitView = _boardView.GetUnitView(unitModel);
                CellView spawnCellView = _boardView.GetCellView(spawnCell);
                
                if (unitView != null)
                {
                    unitView.transform.SetParent(spawnCellView.transform);
                    Vector3 targetPos = _boardView.GetCellPosition(spawnCell);
                    unitView.MoveToPosition(targetPos);
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
                var pathToGate = _pathfindingService.GetPathToGate(_boardModel, currentCell);
                if (pathToGate.Count > 0 && !IsPathBlocked(pathToGate, unit.TeamOwner))
                {
                    results.Add(pathToGate.Last());
                }
            }

            //Đá tiến
            var forwardPath = _pathfindingService.GetPath(_boardModel, currentCell, diceValue);
            if (forwardPath.Count > 0 && !IsPathBlocked(forwardPath, unit.TeamOwner))
            {
                results.Add(forwardPath.Last());
            }

            //Đá hậu
            var reversePath = _pathfindingService.GetReversePath(_boardModel, currentCell, diceValue);
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
            _boardView.HighlightCells(cellModels);
        }

        public void ClearAllHighlights()
        {
            _boardView.ClearAllHighlights();
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
            if (unitModel.State == UnitState.Moving) 
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
            if (currentCell != null)
            {
                currentCell.Clear();
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy ô cũ của Unit {unit.Id}");
            }

            targetCell.Register(unit);

            if (targetCell.Structure == CellStructure.Home)
            {
                unit.SetState(UnitState.InHome);
            }
        }

        private CellModel GetCurrentCellOfUnit(UnitModel unit)
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
                _boardSetting.RedCellCount,
                _boardSetting.YellowCellCount,
                _boardSetting.PurpleCellCount
            );

            OnBoardGenerated?.Invoke(_boardModel);
        }
        private void GenerateUnits()
        {
            _allUnits = _unitModelGenerationService.CreateAllUnits();
            OnAllUnitsGenerated?.Invoke(_allUnits);
        }
        #endregion

    }
}