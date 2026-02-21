using MADP.Models;
using MADP.Services.AI.Interfaces;

namespace MADP.Services.AI
{
    public class AggressiveBotStrategy : IBotStrategy
    {
        public (UnitModel, CellModel) DecideMove(TeamColor team, int diceValue, BoardModel board)
        {
            return (null, null);
        }
    }
}