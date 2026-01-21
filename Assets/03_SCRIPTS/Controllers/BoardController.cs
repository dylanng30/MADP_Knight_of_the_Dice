using System;
using System.Collections.Generic;
using MADP.Models;
using MADP.Services;
using MADP.Settings;
using MADP.Utilities;
using MADP.Views;
using UnityEngine;

namespace MADP.Controllers
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] private BoardSetting _boardSetting;
        [SerializeField] private CellMaterialSetting _materialSetting;
        
        [Header("--- REFERENCES ---")]
        [SerializeField] private CellView cellPrefab;
        [SerializeField] private UnitView unitPrefab;
        [SerializeField] private Transform container;
        
        
        private Dictionary<CellModel, CellView> _cellViewMapper;
        private BoardGenerationService _boardGenerationService;
        private BoardLayoutService _boardLayoutService;
        private BoardRotationService _boardRotationService;
        
        private Dictionary<UnitModel, UnitView> _unitViewMapper;
        private Dictionary<TeamColor, List<UnitModel>> _allUnits;
        private UnitGenerationService _unitGenerationService;
        
        private void Start()
        {
            _cellViewMapper =  new Dictionary<CellModel, CellView>();
            
            _boardGenerationService = new BoardGenerationService();
            _boardLayoutService = new BoardLayoutService();
            _boardRotationService = new BoardRotationService();
            
            _unitGenerationService = new UnitGenerationService();
            
            GenerateBoard();
        }

        private void Update()
        {
            if(container == null) return;
            //_boardRotationService.Update(cellContainer);
        }

        #region ---BOARD---

                public void GenerateBoard()
        {
            Reset();
            
            var fullBoardModel = _boardGenerationService.CreateFullBoard(
                _boardSetting.RedCellCount,
                _boardSetting.YellowCellCount,
                _boardSetting.PurpleCellCount
            );

            for (int i = 0; i < fullBoardModel.AroundCells.Count; i++)
            {
                var aroundCellModel = fullBoardModel.AroundCells[i];
                Vector3 pos = _boardLayoutService.GetMainCellPosition(i);
                CreateCellView(aroundCellModel, pos);
            }

            foreach (var homeCellModel in fullBoardModel.HomeCells)
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
            Material mat = GetMaterial(model);

            CellView view = Instantiate(cellPrefab, container);
            view.Setup(model);
            view.transform.localPosition = position;
            view.Renderer.material = mat;
            
            _cellViewMapper.Add(model, view);
        }
        private void Reset()
        {
            _boardLayoutService.Reset();
            
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            
            _cellViewMapper.Clear();
        }
        private Material GetMaterial(CellModel model)
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

        #endregion

        #region ---UNIT---

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
            UnitView unitView = Instantiate(unitPrefab, container);
        }

        #endregion
    }
}