using System.Collections.Generic;
using MADP.Models;
using MADP.Services.AI.Interfaces;
using MADP.Settings;
using UnityEngine;

namespace MADP.Services.AI
{
    public class BotDecisionService
    {
        private readonly Dictionary<TeamColor, IBotBrain> _bots = new();

        public void RegisterBotStrategy(TeamColor team, IBotBrain botBrain)
        {
            _bots[team] = botBrain;
        }

        public (UnitModel Unit, CellModel Destination) GetBestMove(TeamColor team, int diceValue, BoardModel board)
        {
            if (_bots.TryGetValue(team, out var brain))
                return brain.DecideMove(team, diceValue, board);
            
            return (null, null);
        }

        public List<ItemDataSO> GetShoppingList(TeamColor team, int currentGold, int availableSlots, List<ItemDataSO> shopItems)
        {
            if (_bots.TryGetValue(team, out var brain))
                return brain.DecidePurchases(team, currentGold, availableSlots, shopItems);
            
            return new List<ItemDataSO>();
        }

        public List<(ItemDataSO Item, UnitModel TargetUnit)> GetItemUsageDecisions(TeamColor team, List<ItemDataSO> inventory, BoardModel board)
        {
            if (_bots.TryGetValue(team, out var brain))
                return brain.DecideItemUsage(team, inventory, board);
            
            return new List<(ItemDataSO, UnitModel)>();
        }
    }
}