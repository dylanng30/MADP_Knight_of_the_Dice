using System.Collections.Generic;
using MADP.Controllers;
using MADP.Models;
using MADP.Services.AI.Interfaces;
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
    }
}