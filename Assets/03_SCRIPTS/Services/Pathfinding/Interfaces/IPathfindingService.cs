using System.Collections.Generic;
using MADP.Models;

namespace MADP.Services.Pathfinding.Interfaces
{
    public interface IPathfindingService
    {
        List<CellModel> GetReversePath(BoardModel board, CellModel startCell, int steps);
        List<CellModel> GetPath(BoardModel board, CellModel startCell, int steps);
        List<CellModel> GetPathToGate(BoardModel board, CellModel startCell);
    }
}