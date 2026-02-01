using System.Collections.Generic;
using MADP.Controllers;
using MADP.Models;
using MADP.Services;
using MADP.Settings;
using UnityEngine;

namespace MADP.Views
{
    public class BoardView : MonoBehaviour
    {
        [Header("---SETTING---")]
        [SerializeField] private CellMaterialSetting _materialSetting;
        
        [Space(10)]
        [SerializeField] private CellView _cellViewPrefab;
        [SerializeField] private UnitView _unitViewPrefab;
        
        //Controllers
        private BoardController _controller;
        
        //Services
        private BoardLayoutService _boardLayoutService = new();
        
        //Mapper
        private Dictionary<CellModel, CellView> _cellMap = new();
        private Dictionary<UnitModel, UnitView> _unitMap = new();

        private List<CellView> _currentHighlightedCells = new();
        public void Initialize(BoardController controller)
        {
            _controller = controller;

            _controller.OnBoardGenerated += GenerateBoard;
            _controller.OnAllUnitsGenerated += GenerateUnits;
        }

        public void UnitReturnNest(UnitModel model)
        {
            var pos = _boardLayoutService.GetUnitPositionInCage(model.TeamOwner, model.Id);
            _unitMap[model].transform.localPosition = pos;
        }
        public void Reset()
        {
            _boardLayoutService.Reset();
            
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            _cellMap.Clear();
            _unitMap.Clear();
        }

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
        
        public CellView GetCellView(CellModel cellModel) 
            => _cellMap.ContainsKey(cellModel) ? _cellMap[cellModel] : null;

        public UnitView GetUnitView(UnitModel unitModel)
            => _unitMap.ContainsKey(unitModel) ? _unitMap[unitModel] : null;
        
        public Vector3 GetCellPosition(CellModel cellModel)
        {
            return _cellMap.TryGetValue(cellModel, out var view) ? view.GetUnitPosition() : Vector3.zero;
        }

        public List<Vector3> GetPath(List<CellModel> pathModel)
        {
            List<Vector3> path = new List<Vector3>();
            foreach (var cell in pathModel)
            {
                if (_cellMap.TryGetValue(cell, out var view))
                {
                    path.Add(view.GetUnitPosition()); 
                }
            }
            return path;
        }
        
        #region ---BOARD---
        private void GenerateBoard(BoardModel boardModel)
        {
            for (int i = 0; i < boardModel.AroundCells.Count; i++)
            {
                var aroundCellModel = boardModel.AroundCells[i];
                Vector3 pos = _boardLayoutService.GetMainCellPosition(i);
                GenerateCellView(aroundCellModel, pos);
            }

            foreach (var homeCellModel in boardModel.HomeCells)
            {
                var color = homeCellModel.Key;
                var homeCells = homeCellModel.Value;
                for (int i = 0; i < homeCells.Count; i++)
                {
                    Vector3 pos = _boardLayoutService.GetHomeCellPosition(color, i);
                    GenerateCellView(homeCells[i], pos); 
                }
            }
        }
        
        private void GenerateCellView(CellModel model, Vector3 position)
        {
            Material mat = GetCellMaterial(model);
            CellView view = Instantiate(_cellViewPrefab, this.transform);
            view.Setup(model);
            view.transform.localPosition = position;
            view.Renderer.material = mat;
            
            _cellMap.Add(model, view);
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
        #endregion

        #region ---UNITS---
        private void GenerateUnits(Dictionary<TeamColor, List<UnitModel>> allUnits)
        {
            foreach (var team in allUnits)
            {
                TeamColor color = team.Key;
                List<UnitModel> units = team.Value;

                foreach (var unit in units)
                {
                    GenerateUnitView(unit);
                }
            }
        }

        private void GenerateUnitView(UnitModel model)
        {
            UnitView view = Instantiate(_unitViewPrefab, this.transform);
            view.Setup(model);
            
            var pos = _boardLayoutService.GetUnitPositionInCage(model.TeamOwner, model.Id);
            view.transform.localPosition = pos;
            
            var mat = GetUnitMaterial(model.TeamOwner);
            view.Renderer.material = mat;
            
            _unitMap.Add(model, view);
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