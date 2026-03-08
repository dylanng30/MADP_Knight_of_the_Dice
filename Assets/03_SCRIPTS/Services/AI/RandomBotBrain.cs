using System.Collections.Generic;
using System.Linq;
using MADP.Controllers;
using MADP.Models;
using MADP.Services.AI.Interfaces;
using MADP.Settings;
using UnityEngine;

namespace MADP.Services.AI
{
    public class RandomBotBrain : IBotBrain
    {
        private readonly BoardController _boardController;
        
        public RandomBotBrain(BoardController boardController)
        {
            _boardController = boardController;
        }
        
        public (UnitModel Unit, CellModel Destination) DecideMove(TeamColor team, int diceValue, BoardModel board)
        {
            List<UnitModel> units = _boardController.GetAllUnitsByColor(team);
            List<(UnitModel, CellModel)> validMoves = new List<(UnitModel, CellModel)>();
            
            foreach (var unit in units)
            {
                if (!_boardController.CanInteract(unit, diceValue)) continue;
                if (unit.State == UnitState.Moving && _boardController.IsOvershootingGate(unit, diceValue)) continue; 
                
                if (unit.State == UnitState.InNest)
                {
                    validMoves.Add((unit, null)); 
                }
                else
                {
                    var dests = _boardController.GetPotentialDestinationCell(unit, diceValue);
                    if (dests != null && dests.Count > 0)
                    {
                        validMoves.Add((unit, dests[0])); 
                    }
                }
            }
            
            if (validMoves.Count > 0)
            {
                int randomIndex = Random.Range(0, validMoves.Count);
                return validMoves[randomIndex];
            }

            return (null, null);
        }

        public List<ItemDataSO> DecidePurchases(TeamColor team, int currentGold, int availableSlots, List<ItemDataSO> shopItems)
        {
            List<ItemDataSO> itemsToBuy = new List<ItemDataSO>();
            int remainingGold = currentGold;
            int remainingSlots = availableSlots;
            
            List<ItemDataSO> shuffledShop = shopItems.OrderBy(x => Random.value).ToList();

            foreach (var item in shuffledShop)
            {
                if (remainingSlots > 0 && 
                    remainingGold >= item.Price && 
                    Random.value > 0.5f)
                {
                    itemsToBuy.Add(item);
                    remainingGold -= item.Price;
                    remainingSlots--;
                }
            }

            return itemsToBuy;
        }

        public List<(ItemDataSO Item, UnitModel TargetUnit)> DecideItemUsage(TeamColor team, List<ItemDataSO> inventory, BoardModel board)
        {
            var usageDecisions = new List<(ItemDataSO, UnitModel)>();
            if (inventory == null || inventory.Count == 0) return usageDecisions;
            
            List<UnitModel> activeUnits = _boardController.GetAllUnitsByColor(team)
                .Where(u => u.State == UnitState.Moving && !u.Inventory.IsFull).ToList();

            if (activeUnits.Count == 0) return usageDecisions;

            List<ItemDataSO> itemsToEquip = new List<ItemDataSO>(inventory);

            foreach (var item in itemsToEquip)
            {
                if (Random.value < 0.7f && activeUnits.Count > 0)
                {
                    int randomIndex = Random.Range(0, activeUnits.Count);
                    UnitModel randomUnit = activeUnits[randomIndex];

                    usageDecisions.Add((item, randomUnit));
                    activeUnits.RemoveAt(randomIndex);
                }
            }

            return usageDecisions;
        }
        
    }
}