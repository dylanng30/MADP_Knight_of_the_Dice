using MADP.Models;
using UnityEngine;

namespace MADP.Services
{
    public class NestLayoutService
    {
        private float _cageOffset = 4.0f;
        private float _unitGap = 1f;
        
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
    }
}