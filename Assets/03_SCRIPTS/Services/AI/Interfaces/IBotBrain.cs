using System.Collections.Generic;
using MADP.Models;
using MADP.Settings;

namespace MADP.Services.AI.Interfaces
{
    public interface IBotBrain
    {
        (UnitModel Unit, CellModel Destination) DecideMove(TeamColor team, int diceValue, BoardModel board);
        List<ItemDataSO> DecidePurchases(TeamColor team, int currentGold, int availableSlots, List<ItemDataSO> shopItems);
        List<(ItemDataSO Item, UnitModel TargetUnit)> DecideItemUsage(TeamColor team, List<ItemDataSO> inventory, BoardModel board);
    }
}