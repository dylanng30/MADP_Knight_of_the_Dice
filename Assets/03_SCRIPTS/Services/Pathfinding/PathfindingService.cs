using System.Collections.Generic;
using MADP.Models;
using MADP.Services.Pathfinding.Interfaces;

namespace MADP.Services.Pathfinding
{
    public class PathfindingService : IPathfindingService
    {
        public List<CellModel> GetPathToHome(BoardModel board, CellModel startCell, int steps)
        {
            List<CellModel> path = new List<CellModel>();
            var team = startCell.TeamOwner;
            var homeCells = board.HomeCells[team];

            int startIndex = 0;
            if (startCell.Structure == CellStructure.Home) startIndex = startCell.Index;

            for (int i = startIndex; i < steps && i < homeCells.Count; i++)
            {
                path.Add(homeCells[i]);
            }

            return path;
        }
        public List<CellModel> GetReversePath(BoardModel board, CellModel startCell, int steps)
        {
            List<CellModel> path = new List<CellModel>();
            var aroundCells = board.AroundCells;
            int currentIndex = aroundCells.IndexOf(startCell);
            path.Add(startCell);
            bool hasSpawnCell = false;
            for (int i = 1; i <= steps; i++)
            {
                var rawIndex = (currentIndex - i + aroundCells.Count) % aroundCells.Count;
                
                if (aroundCells[rawIndex].Structure == CellStructure.Spawn)
                {
                    hasSpawnCell = true;
                    if (startCell.HasUnit && startCell.Unit.TeamOwner == aroundCells[rawIndex].TeamOwner)
                    {
                        path.Add(aroundCells[rawIndex]);
                    }
                }
                
                int finalIndex = hasSpawnCell ? (rawIndex - 1 + aroundCells.Count) % aroundCells.Count : rawIndex;
                path.Add(aroundCells[finalIndex]);
            }
            return path;
        }

        public List<CellModel> GetPath(BoardModel board, CellModel startCell, int steps)
        {
            List<CellModel> path = new List<CellModel>();
            var aroundCells = board.AroundCells;
            
            int currentIndex = aroundCells.IndexOf(startCell);
            path.Add(startCell);
            
            bool hasSpawnCell = false;
            for (int i = 1; i <= steps; i++)
            {
                var rawIndex = (currentIndex + i) % aroundCells.Count;
                
                if (aroundCells[rawIndex].Structure == CellStructure.Spawn)
                {
                    hasSpawnCell = true;
                }

                int finalIndex = hasSpawnCell ? (rawIndex + 1) % aroundCells.Count : rawIndex;
                path.Add(aroundCells[finalIndex]);
            }
            return path;
        }

        public List<CellModel> GetPathToGate(BoardModel board, CellModel startCell)
        {
            List<CellModel> path = new List<CellModel>();
            var aroundCells = board.AroundCells;
            int currentIndex = aroundCells.IndexOf(startCell);
            path.Add(startCell);
            
            for (int i = 1; i < aroundCells.Count; i++)
            {
                int nextIndex = (currentIndex + i) % aroundCells.Count;
                CellModel nextCell = aroundCells[nextIndex];
                if(nextCell.Structure != CellStructure.Spawn)
                    path.Add(nextCell);
                if (nextCell.Structure == CellStructure.Gate)
                {
                    return path;
                }
            }
            return new List<CellModel>();
        }
    }
}