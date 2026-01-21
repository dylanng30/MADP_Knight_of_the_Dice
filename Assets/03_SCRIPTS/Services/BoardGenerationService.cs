using System.Collections.Generic;
using MADP.Models;
using MADP.Settings;
using MADP.Utilities;
using UnityEngine;

namespace MADP.Services
{
    public class BoardService
    {
        private HashSet<int> _redIndexes;
        private HashSet<int> _yellowIndexes;
        private HashSet<int> _purpleIndexes;

        public List<CellModel> CreateBoard(Transform container, int redCount, int yellowCount, int purpleCount)
        {
            List<CellModel> models = new();

            Reset();
            CreateSpecialCellIndexes(redCount, yellowCount, purpleCount);
            
            for (int i = 0; i < Constants.DefaultCellCount; i++)
            {
                CellType type = GetCellTypeByIndex(i);
                TeamColor color = GetTeamColorByIndex(i);
                CellModel model = new CellModel(i, type, color);
                models.Add(model);
            }
            
            return models;
        }

        private void Reset()
        {
            if (_redIndexes == null) _redIndexes = new HashSet<int>();
            if (_yellowIndexes == null) _yellowIndexes = new HashSet<int>();
            if (_purpleIndexes == null) _purpleIndexes = new HashSet<int>();
            
            _redIndexes.Clear();
            _yellowIndexes.Clear();
            _purpleIndexes.Clear();
        }
        
        private void CreateSpecialCellIndexes(int redCount, int yellowCount, int purpleCount)
        {
            List<int> availableIndexes = new List<int>();

            for (int i = 0; i < Constants.DefaultCellCount; i++)
            {
                int except = i % 14;
                if (except == 0 || except == 13)
                    continue;

                availableIndexes.Add(i);
            }
            
            int totalSpecialCells =  redCount + yellowCount + purpleCount;

            if (totalSpecialCells > availableIndexes.Count)
            {
                Debug.LogError("Số ô đặc biệt nhiều hơn số ô bình thường");
                return;
            }

            availableIndexes.Shuffle();

            if (redCount > 0)
            {
                for (int i = 0; i < redCount; i++)
                {
                    _redIndexes.Add(availableIndexes[i]);
                }
            }

            if (yellowCount > 0)
            {
                for (int i = 0; i < yellowCount; i++)
                {
                    _yellowIndexes.Add(availableIndexes[redCount + i]);
                }
            }
            
            if (purpleCount > 0)
            {
                for (int i = 0; i < purpleCount; i++)
                {
                    _purpleIndexes.Add(availableIndexes[redCount + yellowCount + i]);
                }
            }
        }
        
        private CellType GetCellTypeByIndex(int index)
        {
            if(_redIndexes.Contains(index)) return CellType.Red;
            if(_yellowIndexes.Contains(index)) return CellType.Yellow;
            if(_purpleIndexes.Contains(index)) return CellType.Purple;
            
            int typeIndex = index % 14;
            if(typeIndex == 0) return CellType.Home;
            if(typeIndex == 13) return CellType.Spawn;

            return CellType.Normal;
        }
        
        private TeamColor GetTeamColorByIndex(int index)
        {
            if(index == 0 || index == 55)
                return TeamColor.Red;
            if(index == 14 || index == 13)
                return TeamColor.Green;
            if(index == 28 || index == 27)
                return TeamColor.Yellow;    
            if(index == 42 || index == 41)
                return TeamColor.Blue;
            
            return TeamColor.None;
        }
    }
}

