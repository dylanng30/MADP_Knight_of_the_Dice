using System;
using System.Collections.Generic;
using MADP.Controllers;
using MADP.Models;
using MADP.Services;
using MADP.Settings;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Views
{
    public class BoardView : MonoBehaviour
    {
        [Header("---SETTING---")]
        [SerializeField] private CellView _cellViewPrefab;
        
        [SerializeField] private UnitView firstUnitViewPrefab;
        [SerializeField] private UnitView secondUnitViewPrefab;
        [SerializeField] private UnitView thirdUnitViewPrefab;
        [SerializeField] private UnitView fourthUnitViewPrefab;
        
        //Controllers
        private BoardController _controller;
        
        //Services
        private BoardLayoutService _boardLayoutService = new ();
        
        //Mapper
        private Dictionary<CellModel, CellView> _cellMap = new();
        private Dictionary<UnitModel, UnitView> _unitMap = new();

        private List<CellView> _currentHighlightedCells = new();
        private MapType _currentMapType;
        private TeamColorDatabaseSO _teamColorDB;

        public void Initialize(
            BoardController controller, 
            Dictionary<TeamColor, int> teamToBaseMap,
            MapType mapType,
            TeamColorDatabaseSO teamColorDB)
        {
            _controller = controller;
            _currentMapType = mapType;

            _teamColorDB = teamColorDB;
            
            _boardLayoutService.Initialize(teamToBaseMap);

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
            ClearAllHighlights();

            if (cellModels.Count <= 0)
                return;

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
            CellView view = Instantiate(_cellViewPrefab, this.transform);
            view.Setup(model);
            view.transform.localPosition = position;
            
            if (model.Structure == CellStructure.Home)
            {
                position.y = 0; 
                Quaternion targetRotation = Quaternion.LookRotation(-position);
                view.transform.rotation = targetRotation;
            }
            
            
            Color finalColor = Color.white;

            if (model.TeamOwner != TeamColor.None)
            {
                finalColor = _teamColorDB.GetTeamColor(model.TeamOwner, Priority.Primary);
            }
            else if (model.Attribute != CellAttribute.None)
            {
                finalColor = GetAttributeColor(model.Attribute);
            }
            else
            {
                bool isEvenIndex = model.Index % 2 == 0;
                finalColor = GetMapThemeColor(_currentMapType, isEvenIndex);
            }
            
            view.Renderer.material.color = finalColor;
            
            _cellMap.Add(model, view);
        }
        
        private Color GetAttributeColor(CellAttribute attribute)
        {
            if (_teamColorDB == null) return Color.gray;

            return attribute switch
            {
                CellAttribute.Red => _teamColorDB.GetTeamColor(TeamColor.Red, Priority.Primary),
                CellAttribute.Yellow => _teamColorDB.GetTeamColor(TeamColor.Red, Priority.Primary),
                CellAttribute.Purple => _teamColorDB.GetTeamColor(TeamColor.Red, Priority.Primary),
                CellAttribute.Blue => _teamColorDB.GetTeamColor(TeamColor.Red, Priority.Primary),
                CellAttribute.Green => _teamColorDB.GetTeamColor(TeamColor.Red, Priority.Primary),
                _ => Color.gray
            };
        }

        private Color GetMapThemeColor(MapType mapType, bool isEvenIndex)
        {
            if (_teamColorDB == null || _teamColorDB.MapSettings == null || _teamColorDB.MapSettings.Count <= 0) 
                return Color.gray;

            foreach (var setting in _teamColorDB.MapSettings)
            {
                if (setting.MapType == mapType)
                    return isEvenIndex ? setting.PrimaryColor : setting.SecondaryColor;
            }

            return Color.gray;
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
            UnitView unitViewPrefab = GetUnitViewPrefab(model.Id);
            UnitView view = Instantiate(unitViewPrefab, this.transform);
            view.Setup(model);
            
            var pos = _boardLayoutService.GetUnitPositionInCage(model.TeamOwner, model.Id);
            view.transform.localPosition = pos;
            
            Vector3 defaultDirection = Vector3.zero - view.transform.position;
            view.Rotate(defaultDirection);
            
            //Hướng nhìn ban đầu
            Vector3 directionToTarget = Vector3.zero - view.transform.position;
            view.transform.LookAt(Vector3.zero);
            
            if(view.PrimarySign.Count > 0)
                foreach (var sign in view.PrimarySign)
                    sign.materials[sign.materials.Length - 1].color = GetUnitColor(model, Priority.Primary);
            
            if(view.SecondarySign.Count > 0)
                foreach (var sign in view.SecondarySign)
                    sign.materials[sign.materials.Length - 1].color = GetUnitColor(model, Priority.Secondary);
            
            if(view.TertiarySign.Count > 0)
                foreach (var sign in view.TertiarySign)
                    sign.materials[sign.materials.Length - 1].color = GetUnitColor(model, Priority.Tertiary);
            
            _unitMap.Add(model, view);
        }

        private Color GetUnitColor(UnitModel model, Priority priority)
        {
            return _teamColorDB.GetTeamColor(model.TeamOwner, priority);
        }

        private UnitView GetUnitViewPrefab(int id)
        {
            switch (id)
            {
                case 0: return firstUnitViewPrefab;
                case 1: return secondUnitViewPrefab;
                case 2: return thirdUnitViewPrefab;
                case 3: return fourthUnitViewPrefab;
                default: return firstUnitViewPrefab;
            }
        }
        #endregion
    }
}