using MADP.Models;
using UnityEngine;

namespace MADP.Services
{
    public class BoardLayoutService
    {
        //Cell
        private float _cellSize = 1f;
        private Vector3 _offset = new Vector3(0, 0, 7);
        //Unit
        private float _cageOffset = 4.0f;
        private float _unitGap = 1f;
        
        private Vector3 _currentPosition;
        private Vector3 _currentDirection;

        public Vector3 GetUnitPositionInCage(TeamColor teamColor, int unitId)
        {
            Vector3 stableCenter = GetCageCenter(teamColor);
            
            float xOffset = (unitId % 2 == 0 ? -1 : 1) * _unitGap / 2;
            float zOffset = (unitId < 2 ? 1 : -1) * _unitGap / 2;

            return stableCenter + new Vector3(xOffset, 0, zOffset);
        }
        private Vector3 GetCageCenter(TeamColor color)
        {
            switch (color)
            {
                case TeamColor.Yellow:    
                    return new Vector3(_cageOffset, 0, -_cageOffset); // Góc dưới phải
                case TeamColor.Green:  
                    return new Vector3(-_cageOffset, 0, -_cageOffset); // Góc dưới trái
                case TeamColor.Red: 
                    return new Vector3(-_cageOffset, 0, _cageOffset);  // Góc trên trái
                case TeamColor.Blue:   
                    return new Vector3(_cageOffset, 0, _cageOffset);   // Góc trên phải
                default: return Vector3.zero;
            }
        }

        public void Reset()
        {
            _currentPosition = _offset;
            _currentDirection = Vector3.left;
        }
        
        public Vector3 GetMainCellPosition(int currentIndex)
        {
            int index = currentIndex % 14;
            
            _currentPosition += _currentDirection;
            if (index == 0 || index == 12)
                _currentDirection = Quaternion.Euler(0, -90, 0) * _currentDirection;
            else if (index == 6)
                _currentDirection = Quaternion.Euler(0, 90, 0) * _currentDirection;
            
            return _currentPosition;
        }

        public Vector3 GetHomeCellPosition(TeamColor color, int stepIndex)
        {
            Vector3 startPos = Vector3.zero; 
            Vector3 direction = Vector3.zero;
            
            switch (color)
            {
                case TeamColor.Red: 
                    direction = Vector3.forward; 
                    break;
                case TeamColor.Blue:
                    direction = Vector3.right;
                    break;
                case TeamColor.Yellow:
                    direction = Vector3.back;
                    break;
                case TeamColor.Green:
                    direction = Vector3.left;
                    break;
            }
            
            return startPos + (direction * (6 - stepIndex) * _cellSize);
        }
    }
}