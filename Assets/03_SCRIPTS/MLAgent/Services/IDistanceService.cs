using MADP.Models;

namespace _03_SCRIPTS.MLAgent.Services
{
    public interface IDistanceService
    {
        int CalculateDistanceToGate(UnitModel unit, BoardModel board);
    }
}
