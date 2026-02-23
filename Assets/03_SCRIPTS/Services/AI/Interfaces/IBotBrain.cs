using MADP.Models;

namespace MADP.Services.AI.Interfaces
{
    public interface IBotBrain
    {
        (UnitModel Unit, CellModel Destination) DecideMove(TeamColor team, int diceValue, BoardModel board);
    }
}