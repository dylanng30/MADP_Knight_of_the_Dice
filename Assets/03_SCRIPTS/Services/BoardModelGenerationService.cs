using System.Collections.Generic;
using MADP.Models;
using MADP.Settings;
using MADP.Utilities;
using MADP.Views;
using UnityEngine;

namespace MADP.Services
{
    public class BoardModelGenerationService
    {
        private HashSet<int> _redIndexes;
        private HashSet<int> _yellowIndexes;
        private HashSet<int> _purpleIndexes;

        public BoardModel CreateFullBoard(int redCount, int yellowCount, int purpleCount)
        {
            BoardModel boardModel = new BoardModel();
            
            boardModel.AroundCells = CreateAroundCells(redCount, yellowCount, purpleCount);
            
            boardModel.HomeCells = new  Dictionary<TeamColor, List<CellModel>>();
            boardModel.HomeCells.Add(TeamColor.Red, CreateHomeCells(TeamColor.Red));
            boardModel.HomeCells.Add(TeamColor.Blue, CreateHomeCells(TeamColor.Blue));
            boardModel.HomeCells.Add(TeamColor.Yellow, CreateHomeCells(TeamColor.Yellow));
            boardModel.HomeCells.Add(TeamColor.Green, CreateHomeCells(TeamColor.Green));
            
            return boardModel;
        }
        
        private List<CellModel> CreateAroundCells(int redCount, int yellowCount, int purpleCount)
        {
            List<CellModel> aroundCells = new List<CellModel>();

            Reset();
            CreateSpecialCellIndexes(redCount, yellowCount, purpleCount);
            
            for (int i = 0; i < 56; i++)
            {
                (CellStructure structure, TeamColor owner) = IdentifyStructure(i);
                CellAttribute attribute = IdentifyAttribute(i);
                var newCellModel = new CellModel(i, structure, attribute, owner);
                aroundCells.Add(newCellModel);
            }

            return aroundCells;
        }
        
        private List<CellModel> CreateHomeCells(TeamColor color)
        {
            List<CellModel> homeCells = new List<CellModel>();
            
            for (int i = 0; i < 6; i++)
            {
                homeCells.Add(new CellModel(i, CellStructure.Home, CellAttribute.None, color));
            }
            
            return homeCells;
        }
        
        private (CellStructure, TeamColor) IdentifyStructure(int index)
        {
            //RED
            if (index == 0) return (CellStructure.Spawn, TeamColor.Red);
            if (index == 55) return (CellStructure.Gate, TeamColor.Red);
            //GREEN
            if (index == 14) return (CellStructure.Spawn, TeamColor.Green);
            if (index == 13) return (CellStructure.Gate, TeamColor.Green);
            //YELLO
            if (index == 28) return (CellStructure.Spawn, TeamColor.Yellow);
            if (index == 27) return (CellStructure.Gate, TeamColor.Yellow);
            //BLUE
            if (index == 42) return (CellStructure.Spawn, TeamColor.Blue);
            if (index == 41) return (CellStructure.Gate, TeamColor.Blue);
            
            return (CellStructure.Normal, TeamColor.None);
        }
        private CellAttribute IdentifyAttribute(int index)
        {
            if (_redIndexes.Contains(index)) return CellAttribute.Red;
            if (_yellowIndexes.Contains(index)) return CellAttribute.Yellow;
            if (_purpleIndexes.Contains(index)) return CellAttribute.Purple;
            return CellAttribute.None;
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

            for (int i = 0; i < 56; i++)
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
    }
}

