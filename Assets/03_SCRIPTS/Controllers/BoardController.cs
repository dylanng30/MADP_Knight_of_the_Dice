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
        
        public void MoveUnit(UnitModel unitModel, CellModel targetCellModel, Action OnMoveCompleted)
        {
            CellModel currentTargetCellModel = null;
            
            foreach (var cellModel in _cellViewMapper.Keys)
            {
                if (cellModel.Unit == unitModel)
                {
                    Debug.Log($"{unitModel.Id} rời {cellModel.Index}");
                    currentTargetCellModel = cellModel; 
                    currentTargetCellModel.Clear();
                    break;
                }
            }
            
            targetCellModel.Register(unitModel);
            unitModel.MoveTo(targetCellModel.Index);

            var unitView = _unitViewMapper[unitModel];
            if (currentTargetCellModel != null)
            {
                var path = GetPath(currentTargetCellModel, 6);
                var cellView = _cellViewMapper[targetCellModel];
                cellView.SetHighlight(false);
                unitView.transform.SetParent(cellView.transform);
                MoveUA moveUA = new MoveUA(unitView, path);
                ActionSystem.Instance.Perform(moveUA, OnMoveCompleted);
            }
            
            //var targetPos = cellView.GetUnitPosition();
            //unitView.MoveToPosition(targetPos);
            //OnMoveCompleted.Invoke();
        }

        public void SpawnUnit(UnitModel unitModel, Action OnComplete)
        {
            var cellModel = _boardModel.AroundCells.FirstOrDefault(c => 
                c.Structure == CellStructure.Spawn && 
                c.TeamOwner == unitModel.TeamOwner &&
                !c.HasUnit);
            
            if (cellModel == null)
                return;
            
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
            
            return potentialDestinationCells;
        }

        public bool CheckIfAnyMovePossible(TeamColor team, int diceValue)
        {
            List<UnitModel> units = _allUnits[team];

            foreach (UnitModel unit in units)
            {
                if(CanUnitMove(unit, diceValue))
                    return true;
            }
            return false;
        }

        public bool CanUnitMove(UnitModel unitModel, int diceValue)
        {
            if (unitModel.State == UnitState.InNest)
            {
                var spawnCell = _cellViewMapper.Keys.FirstOrDefault(c =>
                    c.Structure == CellStructure.Spawn && c.TeamOwner == unitModel.TeamOwner);
                return diceValue == 6 && !spawnCell.HasUnit;
            }

            if (unitModel.State == UnitState.Moving)
            {
                var currentCellOfUnit = _cellViewMapper.Keys.FirstOrDefault(c => c.Unit == unitModel);
                if (currentCellOfUnit != null)
                {
                    var pathModel = GetPathModel(currentCellOfUnit, diceValue);
                    if(IsPathBlocked(pathModel)) return false;
                    else return true;
                }
            }
            return false;
        }
        

        private bool IsPathBlocked(List<CellModel> pathModel)
        {
            for (int i = 1; i < pathModel.Count; i++)
            {
                if(pathModel[i].HasUnit)
                    return true;
            }
            
            return false;
        }
        #endregion

        #region --- PATH ---
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
            var aroundCellsExceptSpawns = _boardModel.AroundCellsExceptSpawns;
            int currentCellIndex = aroundCellsExceptSpawns.IndexOf(currentCellModel);
            
            if(currentCellIndex != -1)
                path.Add(aroundCellsExceptSpawns[currentCellIndex]);
            
            for (int i = 1; i <= diceValue; i++)
            {
                var index = (currentCellIndex + i) % aroundCellsExceptSpawns.Count;
                path.Add(aroundCellsExceptSpawns[index]);
            }
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
                    case TeamColor.Red: return _materialSetting.RedHome;
                    case TeamColor.Blue: return _materialSetting.BlueHome;
                    case TeamColor.Yellow: return _materialSetting.YellowHome;
                    case TeamColor.Green: return _materialSetting.GreenHome;
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