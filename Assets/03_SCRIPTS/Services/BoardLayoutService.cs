using MADP.Models;
using UnityEngine;

namespace MADP.Services
{
    public class BoardLayoutService
    {
        private float _cellSize = 1f;
        private Vector3 _offset = new Vector3(0, 0, 7);
        
        private Vector3 _currentPosition;
        private Vector3 _currentDirection;

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
                    direction = Vector3.left;
                    break;
                case TeamColor.Yellow:
                    direction = Vector3.back;
                    break;
                case TeamColor.Green:
                    direction = Vector3.right;
                    break;
            }
            
            return startPos + (direction * (stepIndex + 1) * _cellSize);
        }
    }
}