using System.Collections.Generic;
using MADP.Controllers;
using MADP.Models;

namespace MADP.Services
{
    public class BotDecisionService
    {
        private BoardController _boardController;

        public BotDecisionService(BoardController boardController)
        {
            _boardController = boardController;
        }

        public UnitModel GetBestUnitToMove(TeamColor botTeamColor, int diceValue)
        {
            List<UnitModel> botAllUnits = _boardController.GetAllUnitsByColor(botTeamColor);
            List<UnitModel> moveableUnits = new List<UnitModel>();

            foreach (var unit in botAllUnits)
            {
                if (_boardController.CanInteract(unit, diceValue))
                {
                    moveableUnits.Add(unit);
                }
            }
            
            if (moveableUnits.Count == 0) 
                return null;
            
            if(moveableUnits.Count == 1)
                return moveableUnits[0];
            
            UnitModel bestUnit = null;
            int highestScore = -1;

            foreach (var unit in moveableUnits)
            {
                int score = CalculateMoveScore(unit, diceValue);
                if (score > highestScore)
                {
                    highestScore = score;
                    bestUnit = unit;
                }
            }

            return bestUnit;
        }

        private int CalculateMoveScore(UnitModel unit, int diceValue)
        {
            int score = 0;

            if (_boardController.CanUnitMove(unit, diceValue))
            {
                var potentialCells = _boardController.GetPotentialDestinationCell(unit, diceValue);

                if (potentialCells.Count > 0)
                {
                    foreach (var cell in potentialCells)
                    {
                        if (cell.HasUnit && cell.Unit.TeamOwner != unit.TeamOwner)
                            score += 1000;
                        else if (cell.Structure == CellStructure.Gate)
                            score += 500;
                        else
                            score += 500; // Điểm đi tieeps
                    }
                }
            }

            if (_boardController.CanUnitSpawn(unit, diceValue))
                score += 500;
            
            return score;
        }
    }
}