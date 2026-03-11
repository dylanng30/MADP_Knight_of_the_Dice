using System.Linq;
using MADP.Models;

namespace _03_SCRIPTS.MLAgent.Services
{
    public class DistanceService : IDistanceService
    {
        public int CalculateDistanceToGate(UnitModel unit, BoardModel board)
        {
            // Nếu quân cờ chưa ra chuồng hoặc đã vào tới Home thì khoảng cách bước ngoài = 0
            if (unit.State == UnitState.InNest || unit.State == UnitState.InHome)
                return -1;

            // Tìm vị trí hiện tại của quân cờ trên Bàn (AroundCells)
            CellModel currentCell = board.AroundCells.FirstOrDefault(c => c.Unit == unit);
            if (currentCell == null) return -1;

            // Tìm ô Gate (Cổng nhà) của đội nhà
            CellModel gateCell = board.AroundCells.FirstOrDefault(c =>
                c.Structure == CellStructure.Gate && c.TeamOwner == unit.TeamOwner);
            if (gateCell == null) return -1;

            int currentIndex = currentCell.Index;
            int gateIndex = gateCell.Index;
            int totalCells = board.AroundCells.Count;

            // Nếu con cờ đang đứng đúng tại ô Gate của nó
            if (currentIndex == gateIndex)
                return 0;

            // Tính khoảng cách đi thẳng (xuôi chiều kim đồng hồ từ currentIndex đến gateIndex)
            int distance;
            if (gateIndex > currentIndex)
            {
                distance = gateIndex - currentIndex;
            }
            else
            {
                // Đi vòng qua mốc 0
                distance = (totalCells - currentIndex) + gateIndex;
            }

            return distance;
        }
    }
}