using MADP.Models;

namespace MADP.Services.AI.Interfaces
{
    public interface IBotStrategy
    {
        (UnitModel Unit, CellModel Destination) DecideMove(TeamColor team, int diceValue, BoardModel board);
    }
}