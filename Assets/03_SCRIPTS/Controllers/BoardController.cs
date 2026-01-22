using System;
using System.Collections.Generic;
using System.Linq;
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
        
        //SERVICES
        private BoardGenerationService _boardGenerationService;
        private BoardLayoutService _boardLayoutService;
        private BoardRotationService _boardRotationService;
        private UnitGenerationService _unitGenerationService;
        
        
        private DiceService _diceService;
        private CellView _currentHighlightedCell;
        
        //MAPPERS
        private Dictionary<CellModel, CellView> _cellViewMapper;
        private Dictionary<UnitModel, UnitView> _unitViewMapper;
        
        private Dictionary<TeamColor, List<UnitModel>> _allUnits;
        private bool _canRoll = true;

        private void Awake()
        {
            _cellViewMapper =  new Dictionary<CellModel, CellView>();
            _unitViewMapper = new Dictionary<UnitModel, UnitView>();
            
            _boardGenerationService = new BoardGenerationService();
            _boardLayoutService = new BoardLayoutService();
            _boardRotationService = new BoardRotationService();
            _unitGenerationService = new UnitGenerationService();
            _diceService = new DiceService();
        }

        private void Start()
        {
            StartGame();
        }

        private void Update()
        {
            if(container == null  || _boardRotationService == null) 
                return;
            _boardRotationService.Update(container);
            
        }

        public void MoveUnit(UnitModel unitModel, CellModel targetCellModel, Action OnMoveCompleted)
        {
            foreach (var cellModel in _cellViewMapper.Keys)
            {
                if (cellModel.Unit == unitModel)
                {
                    Debug.Log($"{unitModel.Id} rời {cellModel.Index}");
                    cellModel.Clear();
                    break;
                }
            }
            
            targetCellModel.Register(unitModel);
            unitModel.MoveTo(targetCellModel.Index);
            
            var unitView = _unitViewMapper[unitModel];
            var cellView = _cellViewMapper[targetCellModel];
            unitView.transform.SetParent(cellView.transform);
            var targetPos = cellView.GetUnitPosition();
            unitView.MoveToPosition(targetPos);
            cellView.SetHighlight(false);
            OnMoveCompleted.Invoke();
        }

        public void SpawnUnit(UnitModel unitModel, Action OnSpawnCompleted)
        {
            foreach (CellModel cellModel in _cellViewMapper.Keys)
            {
                if (cellModel.Structure == CellStructure.Spawn && 
                    cellModel.TeamOwner == unitModel.TeamOwner &&
                    !cellModel.HasUnit)
                {
                    CellView cellView = _cellViewMapper[cellModel];
                    var unitView = _unitViewMapper[unitModel];
                    unitView.transform.SetParent(cellView.transform);
                    var spawnPos = cellView.GetUnitPosition();
                    unitView.MoveToPosition(spawnPos);
                    cellModel.Register(unitModel);
                    unitModel.SetState(UnitState.Moving);
                    unitView.Collider.enabled = false;
                    OnSpawnCompleted.Invoke();
                    break;
                }
            }
        }
        
        public bool CheckIfAnyMovePossible(TeamColor team, int diceValue)
        {
            List<UnitModel> units = _allUnits[team];
            
            foreach (var unit in units)
            {
                if (CanUnitMove(unit, diceValue)) return true;
            }
            return false;
        }
        public bool CanUnitMove(UnitModel unit, int diceValue)
        {
            if (unit.State == UnitState.InCage)
            {
                return _diceService.CanSpawnUnit(diceValue);
            }
            if (unit.State == UnitState.Moving)
            {
                // TODO: Thêm logic kiểm tra có bị chặn bởi quân địch hay quá đường về đích không
                // Tạm thời trả về true nếu đang đi trên bàn cờ
                return true; 
            }
            return false;
        }
        public CellModel GetDestinationCell(UnitModel unit, int diceValue)
        {
            // Case 1: Ra quân
            if (unit.State == UnitState.InCage)
            {
                // Tìm ô Spawn của màu này
                // (Logic này nên tối ưu bằng cách cache spawn index, ở đây tôi ví dụ tìm trong list)
                return _cellViewMapper.Keys.FirstOrDefault(c => c.Structure == CellStructure.Spawn && c.TeamOwner == unit.TeamOwner);
            }

            // Case 2: Đang di chuyển
            if (unit.State == UnitState.Moving)
            {
                // Giả định logic đi vòng quanh (cần thay bằng logic Path thật của bạn)
                int nextIndex = (unit.CurrentIndex + diceValue) % 56; 
                Debug.Log(nextIndex);
                
                // Tìm CellModel tại index đó
                foreach(var cell in _cellViewMapper.Keys)
                {
                    if (cell.Index == nextIndex && 
                        cell.Structure != CellStructure.Home &&
                        !cell.HasUnit) 
                        return cell;
                }
            }
            
            Debug.Log("Không tìm thấy ô tiềm năng");
            return null;
        }
        
        public void HighlightCell(CellModel cellModel, bool isOn)
        {
            if (cellModel == null) return;
            
            if (_cellViewMapper.TryGetValue(cellModel, out CellView view))
            {
                view.SetHighlight(isOn); 
                
                if(isOn) _currentHighlightedCell = view;
                else _currentHighlightedCell = null;
            }
        }

        public void ClearAllHighlights()
        {
            if (_currentHighlightedCell != null)
            {
                _currentHighlightedCell.SetHighlight(false);
                _currentHighlightedCell = null;
            }
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

        #region ---BOARD---
        public void GenerateBoard()
        {
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