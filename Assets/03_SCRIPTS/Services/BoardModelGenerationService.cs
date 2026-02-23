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
        
        private Dictionary<TeamColor, int> _teamToBaseMap;
        public BoardModel CreateFullBoard(
            int redCount, 
            int yellowCount, 
            int purpleCount, 
            List<LobbySlotModel> activeSlots)
        {
            _teamToBaseMap = new Dictionary<TeamColor, int>();
            foreach (var slot in activeSlots)
            {
                _teamToBaseMap[slot.TeamColor] = slot.SlotIndex;
            }
            
            BoardModel boardModel = new BoardModel();
            
            boardModel.AroundCells = CreateAroundCells(redCount, yellowCount, purpleCount);
            boardModel.HomeCells = new  Dictionary<TeamColor, List<CellModel>>();
            
            foreach (var slot in activeSlots)
            {
                boardModel.HomeCells.Add(slot.TeamColor, CreateHomeCells(slot.TeamColor));
            }
            
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
        
        private (CellStructure, TeamColor) IdentifyStructure(int index)
        {
            if (index % 14 == 0)
            {
                int baseIndex = index / 14;
                TeamColor teamOwner = GetTeamByBaseIndex(baseIndex);
                return (CellStructure.Spawn, teamOwner);
            }
            
            if ((index + 1) % 14 == 0)
            {
                int baseIndex = (index + 1) % 56 / 14; 
                TeamColor teamOwner = GetTeamByBaseIndex(baseIndex);
                return (CellStructure.Gate, teamOwner);
            }
            
            return (CellStructure.Normal, TeamColor.None);
        }
        
        private TeamColor GetTeamByBaseIndex(int baseIndex)
        {
            foreach (var kvp in _teamToBaseMap)
            {
                if (kvp.Value == baseIndex)
                    return kvp.Key;
            }
            return TeamColor.None;
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

