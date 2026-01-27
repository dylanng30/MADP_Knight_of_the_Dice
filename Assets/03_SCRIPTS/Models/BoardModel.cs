using System.Collections.Generic;

namespace MADP.Models
{
    public class BoardModel
    {
        public List<CellModel> AroundCells;
        public List<CellModel> AroundCellsExceptSpawns;
        public Dictionary<TeamColor, List<CellModel>> HomeCells;
    }
}

