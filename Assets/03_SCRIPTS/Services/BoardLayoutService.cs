using System.Collections.Generic;
using MADP.Models;
using MADP.Utilities;
using UnityEngine;

namespace MADP.Services
{
    public class BoardLayoutService
    {
        //Cell
        private float _cellSize = 1f;
        private Vector3 _offset = new Vector3(0, 0, 8);
        //Unit
        private float _cageOffset = 8.0f;
        private float _unitGap = 1.5f;
        
        private Vector3 _currentPosition;
        private Vector3 _currentDirection;
        
        private Dictionary<TeamColor, int> _teamToBaseMap;
        
        public void Initialize(Dictionary<TeamColor, int> teamToBaseMap)
        {
            _teamToBaseMap = teamToBaseMap;
        }

        public Vector3 GetUnitPositionInCage(TeamColor teamColor, int unitId)
        {
            Vector3 stableCenter = GetCageCenter(teamColor);
            
            float xOffset = (unitId % 2 == 0 ? -1 : 1) * _unitGap / 2;
            float zOffset = (unitId < 2 ? 1 : -1) * _unitGap / 2;

            return stableCenter * _cellSize + new Vector3(xOffset, 0, zOffset);
        }
        private Vector3 GetCageCenter(TeamColor color)
        {
            if (_teamToBaseMap == null || !_teamToBaseMap.TryGetValue(color, out int baseIndex)) 
                return Vector3.zero;
            
            return baseIndex switch
            {
                0 => new Vector3(-_cageOffset, 0, _cageOffset), //Trên trái
                1 => new Vector3(-_cageOffset, 0, -_cageOffset), //Dưới trái
                2 => new Vector3(_cageOffset, 0, -_cageOffset), //Dưới phải
                3 => new Vector3(_cageOffset, 0, _cageOffset), //Trên phải
                _ => Vector3.zero
            };
        }

        public void Reset()
        {
            _currentPosition = _offset * _cellSize;
            _currentDirection = Vector3.left * _cellSize;
        }
        
        public Vector3 GetMainCellPosition(int currentIndex)
        {
            int index = currentIndex % Constants.CellCountPerTeam;
            
            _currentPosition += _currentDirection;
            if (index == 1 || index == 13)
                _currentDirection = Quaternion.Euler(0, -90, 0) * _currentDirection;
            else if (index == 7)
                _currentDirection = Quaternion.Euler(0, 90, 0) * _currentDirection;
            
            return _currentPosition;
        }

        public Vector3 GetHomeCellPosition(TeamColor color, int stepIndex)
        {
            if (_teamToBaseMap == null || !_teamToBaseMap.TryGetValue(color, out int baseIndex)) 
                return Vector3.zero;

            Vector3 direction = baseIndex switch
            {
                0 => Vector3.forward, //Nhà 0 đi thẳng lên
                1 => Vector3.left, //Nhà 1 đi sang trái
                2 => Vector3.back, //Nhà 2 đi lùi xuống
                3 => Vector3.right, // Nhà 3 đi sang phải
                _ => Vector3.zero
            };
            
            return Vector3.zero + (direction * (7 - stepIndex) * _cellSize);
        }
    }
}