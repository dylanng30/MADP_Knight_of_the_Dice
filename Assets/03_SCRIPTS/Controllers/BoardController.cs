using System;
using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Models.UnitActions;
using MADP.Services;
using MADP.Settings;
using MADP.Systems;
using MADP.Views;
using UnityEngine;

namespace MADP.Controllers
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] private BoardSetting _boardSetting;
        [SerializeField] private CellMaterialSetting _materialSetting;
        [SerializeField] private CellView cellPrefab;
        [SerializeField] private UnitView unitPrefab;
        [SerializeField] private Transform container;
        
        private Dictionary<CellModel, CellView> _cellViewMapper = new();
        private Dictionary<UnitModel, UnitView> _unitViewMapper = new();
        private Dictionary<TeamColor, List<UnitModel>> _allUnits;
        private List<CellView> _currentHighlightedCells = new();

        private BoardModel _boardModel;

        private BoardModelGenerationService _boardModelGenerationService = new();
        private BoardLayoutService _boardLayoutService = new();
        private UnitGenerationService _unitGenerationService = new();

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            Reset();
            GenerateBoard();
            CreateUnits();
        }
        
        private void Reset()
        {
            _boardLayoutService.Reset();
            
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            
            _unitViewMapper.Clear();
            _cellViewMapper.Clear();
        }

        public CellView GetCellView(CellModel cellModel)
        {
            if (_cellViewMapper.ContainsKey(cellModel))
                return _cellViewMapper[cellModel];

            return null;
        }

        public UnitView GetUnitView(UnitModel unitModel)
        {
            if (_unitViewMapper.ContainsKey(unitModel))
                return _unitViewMapper[unitModel];

            return null;
        }

        public List<UnitModel> GetAllUnitsByColor(TeamColor teamColor)
        {
            return _allUnits[teamColor];
        }
        
        

        #region --- VFX ---
        public void HighlightCells(List<CellModel> cellModels)
        {
            _currentHighlightedCells.Clear();

            if (cellModels.Count <= 0)
            {
                ClearAllHighlights();
                return;
            }

            foreach (var cellModel in cellModels)
            {
                var cellView = GetCellView(cellModel);
                _currentHighlightedCells.Add(cellView);
                cellView.SetHighlight(true);
            }
        }

        public void ClearAllHighlights()
        {
            foreach (var cellView in _currentHighlightedCells)
            {
                cellView.SetHighlight(false);
            }
            _currentHighlightedCells.Clear();
        }
        #endregion

        #region --- GAMEPLAY LOGIC ---
        
        public void MoveUnit(UnitModel unitModel, CellModel targetCellModel, int diceValue, Action OnMoveCompleted)
        {
            CellModel currentTargetCellModel = null;
            
            // Tìm ô hiện tại của Unit
            foreach (var cellModel in _cellViewMapper.Keys)
            {
                if (cellModel.Unit == unitModel)
                {
                    currentTargetCellModel = cellModel; 
                    currentTargetCellModel.Clear();
                    break;
                }
            }
            
            // Cập nhật dữ liệu Model
            targetCellModel.Register(unitModel);
            unitModel.MoveTo(targetCellModel.Index);

            // --- XỬ LÝ VISUAL ---
            if (currentTargetCellModel != null)
            {
                List<Vector3> pathPoints = new List<Vector3>();
                
                bool isSpecialGateJump = (diceValue == 1) && 
                                         (targetCellModel.Structure == CellStructure.Gate) &&
                                         !IsNextCell(currentTargetCellModel, targetCellModel);

                if (isSpecialGateJump)
                {
                    // Lấy đường đi đầy đủ tới Gate (để visual nhảy qua từng ô)
                    var cellPath = GetPathToNextGate(currentTargetCellModel);
                    
                    // Chuyển đổi CellPath sang Vector3 Path
                    foreach (var cell in cellPath)
                    {
                        var view = GetCellView(cell);
                        if (view != null) pathPoints.Add(view.GetUnitPosition());
                    }
                }
                else 
                {
                    // TH2: DI CHUYỂN BÌNH THƯỜNG
                    // Sử dụng logic cũ
                    pathPoints = GetPath(currentTargetCellModel, diceValue);
                }
                
                // Thực hiện Action Di chuyển
                var unitView = _unitViewMapper[unitModel];
                var targetView = _cellViewMapper[targetCellModel];
                
                targetView.SetHighlight(false);
                unitView.transform.SetParent(targetView.transform);
                
                MoveUA moveUA = new MoveUA(unitView, pathPoints);
                ActionSystem.Instance.Perform(moveUA, OnMoveCompleted);
            }
        }
        
        private bool IsNextCell(CellModel cellA, CellModel cellB)
        {
            var list = _boardModel.AroundCells;
            int indexA = list.IndexOf(cellA);
            int indexNext = (indexA + 1) % list.Count;
            return list[indexNext] == cellB;
        }

        public void SpawnUnit(UnitModel unitModel, Action OnComplete)
        {
            var cellModel = _boardModel.AroundCells.FirstOrDefault(c => 
                c.Structure == CellStructure.Spawn && 
                c.TeamOwner == unitModel.TeamOwner &&
                !c.HasUnit);
            
            if (cellModel == null)
                return;

            if(GoldController.Instance.TrySpendGold(unitModel.TeamOwner, unitModel.Cost))
            {
                CellView cellView = _cellViewMapper[cellModel];
                var unitView = _unitViewMapper[unitModel];
                unitView.transform.SetParent(cellView.transform);
                var spawnPos = cellView.GetUnitPosition();
                unitView.MoveToPosition(spawnPos);
                cellModel.Register(unitModel);
                unitModel.SetState(UnitState.Moving);
                unitView.Collider.enabled = false;
                OnComplete.Invoke();
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
            /*if (unitModel.State == UnitState.InNest)
            {
                var spawnCellOfUnitModel = _cellViewMapper.Keys.FirstOrDefault(c => 
                    c.Structure == CellStructure.Spawn && c.TeamOwner == unitModel.TeamOwner);
                
                potentialDestinationCells.Add(spawnCellOfUnitModel);
            }*/

            if (unitModel.State == UnitState.Moving)
            {
                var currentCellOfUnit = _cellViewMapper.Keys.FirstOrDefault(c => c.Unit == unitModel);
                if (currentCellOfUnit != null)
                {
                    bool canJumpToGate = false;
                    
                    if (diceValue == 1)
                    {
                        List<CellModel> pathToGate = GetPathToNextGate(currentCellOfUnit);
                        
                        if (pathToGate.Count > 0 && !IsPathBlocked(pathToGate))
                        {
                            potentialDestinationCells.Add(pathToGate.Last());
                            Debug.Log("Khong bi chan");
                            canJumpToGate = true;
                        }
                    }

                    if (!canJumpToGate)
                    {
                        var pathModel = GetPathModel(currentCellOfUnit, diceValue);
                        if (!IsPathBlocked(pathModel))
                            potentialDestinationCells.Add(pathModel.Last());
                    
                        var reversePathModel = GetReversePathModel(currentCellOfUnit, diceValue);
                        if (!IsPathBlocked(reversePathModel))
                        {
                            var cellModel = reversePathModel.Last();
                            if (cellModel.HasUnit)
                            {
                                var unit = cellModel.Unit;
                                if(unit.TeamOwner != unit.TeamOwner)
                                    potentialDestinationCells.Add(cellModel);
                            }
                        }
                    }
                        
                }
            }
            
            return potentialDestinationCells;
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
            return CanUnitSpawn(unitModel, diceValue) ||
                   CanUnitMove(unitModel, diceValue);
        }

        public bool CanUnitSpawn(UnitModel unitModel, int diceValue)
        {
            if(unitModel.State != UnitState.InNest)
                return false;
            
            var spawnCell = _cellViewMapper.Keys.FirstOrDefault(c =>
                c.Structure == CellStructure.Spawn && c.TeamOwner == unitModel.TeamOwner);

            bool hasEnoughGold = GoldController.Instance.GetGold(unitModel.TeamOwner) >= unitModel.Cost;
            return diceValue == 6 && !spawnCell.HasUnit && hasEnoughGold;
        }

        public bool CanUnitMove(UnitModel unitModel, int diceValue)
        {
            if (unitModel.State != UnitState.Moving)
                return false;
            
            var currentCellOfUnit = _cellViewMapper.Keys.FirstOrDefault(c => c.Unit == unitModel);
            if (currentCellOfUnit != null)
            {
                var pathModel = GetPathModel(currentCellOfUnit, diceValue);
                if(IsPathBlocked(pathModel)) return false;
                else return true;
            }
            
            return false;
        }
        

        private bool IsPathBlocked(List<CellModel> pathModel)
        {
            for (int i = 1; i < pathModel.Count - 1; i++)
            {
                if(pathModel[i].HasUnit)
                    return true;
            }
            
            //Temp -> kiểm tra quân cuối cùng cos phải quân đồng minh không
            if(pathModel[pathModel.Count - 1].HasUnit &&
               pathModel[pathModel.Count - 1].Unit.TeamOwner == pathModel[0].Unit.TeamOwner)
                return true;
            
            return false;
        }
        #endregion

        #region --- PATH ---
        private List<CellModel> GetPathToNextGate(CellModel currentCell)
        {
            List<CellModel> path = new List<CellModel>();
            var aroundCells = _boardModel.AroundCells;
            int currentIndex = aroundCells.IndexOf(currentCell);
            
            for (int i = 1; i < aroundCells.Count; i++)
            {
                int nextIndex = (currentIndex + i) % aroundCells.Count;
                CellModel nextCell = aroundCells[nextIndex];
                path.Add(nextCell);
                if (nextCell.Structure == CellStructure.Gate)
                {
                    return path;
                }
            }

            return new List<CellModel>();
        }
        public List<CellModel> GetReversePathModel(CellModel currentCellModel, int diceValue)
        {
            List<CellModel> path = new List<CellModel>();
            var aroundCellsExceptSpawns = _boardModel.AroundCellsExceptSpawns;
            int currentCellIndex = aroundCellsExceptSpawns.IndexOf(currentCellModel);
            for (int i = 0; i <= diceValue; i++)
            {
                var index = (currentCellIndex - i + aroundCellsExceptSpawns.Count) % aroundCellsExceptSpawns.Count;
                path.Add(aroundCellsExceptSpawns[index]);
            }
            return path;
        }
        
        public List<CellView> GetReversePathView(CellModel currentCellModel, int diceValue)
        {
            List<CellView> path = new List<CellView>();
            var pathModel = GetReversePathModel(currentCellModel, diceValue);
            for (int i = 0; i < pathModel.Count; i++)
            {
                var cellModel = pathModel[i];
                var cellView = GetCellView(cellModel);
                if (cellView != null)
                    path.Add(cellView);
            }
            
            return path;
        }
        
        public List<Vector3> GetReversePath(CellModel currentCellModel, int diceValue)
        {
            List<Vector3> path = new List<Vector3>();
            var pathView = GetReversePathView(currentCellModel, diceValue);
            for (int i = 0; i < pathView.Count; i++)
            {
                var cellView = pathView[i];
                var point = cellView.GetUnitPosition();
                path.Add(point);
            }
            
            return path;
        }
        
        public List<CellModel> GetPathModel(CellModel currentCellModel, int diceValue)
        {
            List<CellModel> path = new List<CellModel>();
            
            var aroundCells = _boardModel.AroundCells;
            int currentCellIndex = aroundCells.IndexOf(currentCellModel);
            path.Add(currentCellModel);
            
            bool hasSpawnCell = false;
            
            for (int i = 1; i <= diceValue; i++)
            {
                var rawIndex = (currentCellIndex + i) % aroundCells.Count;

                if (aroundCells[rawIndex].Structure == CellStructure.Spawn)
                {
                    hasSpawnCell = true;
                }

                int currentIndex = hasSpawnCell ? (rawIndex + 1) % aroundCells.Count : rawIndex;
                path.Add(aroundCells[currentIndex]);
            }
            Debug.Log($"So phan tu: {path.Count}");
            return path;
        }

        public List<CellView> GetPathView(CellModel currentCellModel, int diceValue)
        {
            List<CellView> path = new List<CellView>();
            var pathModel = GetPathModel(currentCellModel, diceValue);
            for (int i = 0; i < pathModel.Count; i++)
            {
                var cellModel = pathModel[i];
                var cellView = GetCellView(cellModel);
                if (cellView != null)
                    path.Add(cellView);
            }
            
            return path;
        }
        
        public List<Vector3> GetPath(CellModel currentCellModel, int diceValue)
        {
            List<Vector3> path = new List<Vector3>();
            var pathView = GetPathView(currentCellModel, diceValue);
            for (int i = 0; i < pathView.Count; i++)
            {
                var cellView = pathView[i];
                var point = cellView.GetUnitPosition();
                path.Add(point);
            }
            
            return path;
        }

        #endregion

        #region --- INITIALIZE ---
        public void GenerateBoard()
        {
            _boardModel = _boardModelGenerationService.CreateFullBoard(
                _boardSetting.RedCellCount,
                _boardSetting.YellowCellCount,
                _boardSetting.PurpleCellCount
            );

            for (int i = 0; i < _boardModel.AroundCells.Count; i++)
            {
                var aroundCellModel = _boardModel.AroundCells[i];
                Vector3 pos = _boardLayoutService.GetMainCellPosition(i);
                CreateCellView(aroundCellModel, pos);
            }

            foreach (var homeCellModel in _boardModel.HomeCells)
            {
                var color = homeCellModel.Key;
                var homeCells = homeCellModel.Value;
                for (int i = 0; i < homeCells.Count; i++)
                {
                    Vector3 pos = _boardLayoutService.GetHomeCellPosition(color, i);
                    CreateCellView(homeCells[i], pos); 
                }
            }
        }
        private void CreateCellView(CellModel model, Vector3 position)
        {
            Material mat = GetCellMaterial(model);
            CellView view = Instantiate(cellPrefab, container);
            view.Setup(model);
            view.transform.localPosition = position;
            view.Renderer.material = mat;
            
            _cellViewMapper.Add(model, view);
        }
        
        private Material GetCellMaterial(CellModel model)
        {
            if (model.Structure == CellStructure.Spawn)
            {
                switch (model.TeamOwner)
                {
                    case TeamColor.Red: return _materialSetting.RedSpawn;
                    case TeamColor.Blue: return _materialSetting.BlueSpawn;
                    case TeamColor.Yellow: return _materialSetting.YellowSpawn;
                    case TeamColor.Green: return _materialSetting.GreenSpawn;
                }
            }

            if (model.Structure == CellStructure.Gate)
            {
                switch (model.TeamOwner)
                {
                    case TeamColor.Red: return _materialSetting.Purple;
                    case TeamColor.Blue: return _materialSetting.Purple;
                    case TeamColor.Yellow: return _materialSetting.Purple;
                    case TeamColor.Green: return _materialSetting.Purple;
                }
            }

            if (model.Structure == CellStructure.Home)
            {
                switch (model.TeamOwner)
                {
                    case TeamColor.Red: return _materialSetting.RedHome;
                    case TeamColor.Blue: return _materialSetting.BlueHome;
                    case TeamColor.Yellow: return _materialSetting.YellowHome;
                    case TeamColor.Green: return _materialSetting.GreenHome;
                }
            }
            
            switch (model.Attribute)
            {
                case CellAttribute.Red: return _materialSetting.Red;
                case CellAttribute.Yellow: return _materialSetting.Yellow;
                case CellAttribute.Purple: return _materialSetting.Purple;
                default: return _materialSetting.Normal;
            }
        }
        
        private void CreateUnits()
        {
            _allUnits = _unitGenerationService.CreateAllUnits();
            
            foreach (var team in _allUnits)
            {
                TeamColor color = team.Key;
                List<UnitModel> units = team.Value;

                foreach (var unit in units)
                {
                    CreateUnitView(unit);
                }
            }
        }

        private void CreateUnitView(UnitModel model)
        {
            UnitView view = Instantiate(unitPrefab, container);
            view.Setup(model);
            
            var pos = _boardLayoutService.GetUnitPositionInCage(model.TeamOwner, model.Id);
            view.transform.localPosition = pos;
            
            var mat = GetUnitMaterial(model.TeamOwner);
            view.Renderer.material = mat;
            
            _unitViewMapper.Add(model, view);
        }

        private Material GetUnitMaterial(TeamColor color)
        {
            switch (color)
            {
                case TeamColor.Red: return _materialSetting.RedHome;
                case TeamColor.Blue: return _materialSetting.BlueHome;
                case TeamColor.Yellow: return _materialSetting.YellowHome;
                case TeamColor.Green: return _materialSetting.GreenHome;
                default: return _materialSetting.Normal;
            }
        }

        #endregion
    }
}