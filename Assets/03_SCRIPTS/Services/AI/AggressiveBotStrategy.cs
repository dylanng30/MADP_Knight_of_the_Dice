using MADP.Models;
using MADP.Services.AI.Interfaces;

namespace MADP.Services.AI
{
    public class AggressiveBotStrategy : IBotStrategy
    {
        public (UnitModel, CellModel) DecideMove(TeamColor team, int diceValue, BoardModel board)
        {
            // Logic tìm nước đi ăn quân đối phương
            // Logic cũ từ BotDecisionService chuyển vào đây
            return (null, null);
        }
    }
}