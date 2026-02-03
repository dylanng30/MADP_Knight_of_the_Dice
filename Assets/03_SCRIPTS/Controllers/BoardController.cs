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
using UnityEngine;

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

        #region --- GAMEPLAY LOGIC ---
        public void MoveUnit(UnitModel unitModel, CellModel targetCellModel, int diceValue, Action OnMoveCompleted)
        {
            CellModel currentCellModel = GetCurrentCellOfUnit(unitModel);
            _boardView.ClearAllHighlights();

            if (targetCellModel.Structure == CellStructure.Home && 
                targetCellModel.TeamOwner == unitModel.TeamOwner)
            {
                Debug.Log($"Unit {unitModel.Id} đã về tới chuồng của team {unitModel.TeamOwner}");
                List<CellModel> homePath = _pathfindingService.GetPathToHome(_boardModel, currentCellModel, diceValue);
                List<Vector3> homeVisualPath = _boardView.GetPath(homePath);

                ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);
                MoveUA moveToHomeUA = new MoveUA(_boardView.GetUnitView(unitModel), homeVisualPath);
                ActionSystem.Instance.Perform(moveToHomeUA, OnMoveCompleted);
                return;
            }

            bool isSpecialGateJump = (diceValue == 1) &&
                                     (targetCellModel.Structure == CellStructure.Gate) &&
                                     !IsNextCell(currentCellModel, targetCellModel);
            
            List<CellModel> pathCells = isSpecialGateJump ?
                _pathfindingService.GetPathToGate(_boardModel, currentCellModel) :
                _pathfindingService.GetPath(_boardModel, currentCellModel, diceValue);
            
            List<Vector3> fullVisualPath = _boardView.GetPath(pathCells);

            if (targetCellModel.HasUnit && targetCellModel.Unit.TeamOwner != unitModel.TeamOwner)
            {
                UnitModel victim = targetCellModel.Unit;
                UnitView victimView = _boardView.GetUnitView(victim);
                UnitView attackerUnitView = _boardView.GetUnitView(unitModel);

                List<Vector3> approachPath = new List<Vector3>(fullVisualPath);

                if (approachPath.Count > 1)
                    approachPath.RemoveAt(approachPath.Count - 1);

                MoveUA approachMoveUA = new MoveUA(attackerUnitView, approachPath);

                CombatResult result = _combatService.SimulateCombat(unitModel, victim);

                AttackUA attackUA = new AttackUA(attackerUnitView, victimView, result.IsVictimDead);
                approachMoveUA.PostActions.Add(attackUA);

                if (result.IsVictimDead)
                {
                    Debug.Log($"Unit {victim.Id} chết. Unit {unitModel.Id} chiếm ô.");

                    victim.TakeDamage(result.DamageDealt);
                    _boardView.UnitReturnNest(victim);
                    victim.Revive();

                    ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);

                    CellView targetCellView = _boardView.GetCellView(targetCellModel);
                    attackerUnitView.transform.SetParent(targetCellView.transform);

                    var winStepPath = new List<Vector3> { approachPath.Last(), fullVisualPath.Last() };
                    MoveUA winMoveUA = new MoveUA(attackerUnitView, winStepPath);
                    attackUA.PostActions.Add(winMoveUA);
                }
                else
                {
                    Debug.Log($"Unit {victim.Id} của team {victim.TeamOwner.ToString()} sống sót. Unit {unitModel.Id} của team {victim.TeamOwner.ToString()} quay về.");

                    victim.TakeDamage(result.DamageDealt);

                    List<Vector3> returnPath = new List<Vector3>(approachPath);
                    returnPath.Reverse();

                    MoveUA returnUA = new MoveUA(attackerUnitView, returnPath);
                    attackUA.PostActions.Add(returnUA);
                }

                ActionSystem.Instance.Perform(approachMoveUA, OnMoveCompleted);
            }
            else
            {
                UnitView unitView = _boardView.GetUnitView(unitModel);
                CellView targetCellView = _boardView.GetCellView(targetCellModel);

                ExecuteMoveSuccess(unitModel, currentCellModel, targetCellModel);

                unitView.transform.SetParent(targetCellView.transform);
                MoveUA moveUA = new MoveUA(unitView, fullVisualPath);
                ActionSystem.Instance.Perform(moveUA, OnMoveCompleted);
            }           
            
        }

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
            List<CellModel> potentialDestinationCells = new List<CellModel>();

            if(unitModel.State == UnitState.InHome)
            {
                var teamCells = _boardModel.HomeCells[unitModel.TeamOwner];
                var currentCell = teamCells.FirstOrDefault(c => c.Unit == unitModel);
                if(currentCell != null)
                {
                    int nextCellIndex = currentCell.Index + 1;
                    if (nextCellIndex < teamCells.Count)
                    {
                        if (nextCellIndex + 1 == diceValue)
                        {
                            if (!teamCells[nextCellIndex].HasUnit)
                            {
                                potentialDestinationCells.Add(teamCells[nextCellIndex]);
                            }
                        }
                    }
                }
            }
            else if (unitModel.State == UnitState.Moving)
            {
                var currentCell = _boardModel.AroundCells.FirstOrDefault(c => c.Unit == unitModel);
                if (currentCell != null)
                {
                    //Đã về tới chuồng
                    if(currentCell.Structure == CellStructure.Gate && currentCell.TeamOwner == unitModel.TeamOwner)
                    {
                        List<CellModel> homeCells = _boardModel.HomeCells[currentCell.TeamOwner];
                        if(homeCells == null || homeCells.Count == 0)
                        {
                            Debug.Log($"Home cell của team {currentCell.TeamOwner} không tồn tại");
                            return potentialDestinationCells;
                        }

                        //Duyệt các ô trong nhà của đội tương ứng
                        for (int i = 0; i < diceValue; i++)
                        {
                            var homeCell = homeCells[i];
                            int virtualSteps = i + 1;

                            //Kiểm tra từ ô 1 đến ô (diceValue -1)
                            //Nếu có thì không tính ô vào potential
                            if (virtualSteps < diceValue)
                            {
                                if(homeCell.HasUnit)
                                    break;
                            }
                            else if( virtualSteps == diceValue)
                            {
                                if(!homeCell.HasUnit)
                                    potentialDestinationCells.Add(homeCell);
                            }
                        }
                    }
                    //Chưa về chuồng
                    else
                    {
                        bool canJumpToGate = false;

                        if (diceValue == 1)
                        {
                            var pathToGate = _pathfindingService.GetPathToGate(_boardModel, currentCell);

                            if (pathToGate.Count > 0 && !IsPathBlocked(pathToGate, unitModel.TeamOwner))
                            {
                                potentialDestinationCells.Add(pathToGate.Last());
                                canJumpToGate = true;
                            }
                        }

                        if (!canJumpToGate)
                        {
                            var pathModel = _pathfindingService.GetPath(_boardModel, currentCell, diceValue);
                            if (pathModel.Count > 0 && !IsPathBlocked(pathModel, unitModel.TeamOwner))
                            {
                                potentialDestinationCells.Add(pathModel.Last());
                            }

                            var reversePathModel = _pathfindingService.GetReversePath(_boardModel, currentCell, diceValue);
                            if (reversePathModel.Count > 0 && !IsPathBlocked(reversePathModel, unitModel.TeamOwner))
                            {
                                var cellModel = reversePathModel.Last();
                                if (cellModel.HasUnit)
                                {
                                    var unit = cellModel.Unit;
                                    if (unit.TeamOwner != unitModel.TeamOwner)
                                        potentialDestinationCells.Add(cellModel);
                                }
                            }
                        }
                        
                    }   
                    
                }
            }
            
            return potentialDestinationCells;
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