using System.Collections.Generic;

namespace MADP.Models
{
    public class FullBoardModel
    {
        public List<CellModel> AroundCells;
        public Dictionary<TeamColor, List<CellModel>> HomeCells;
    }
}

