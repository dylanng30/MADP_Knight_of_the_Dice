using System.Collections.Generic;
using MADP.Controllers;
using MADP.Models;
using MADP.Services.AI.Interfaces;
using MADP.Settings;
using UnityEngine;

namespace MADP.Services.AI
{
    public class SmartBotBrain : IBotBrain
    {
        private readonly BoardController _boardController;
        private readonly BotProfileSO _profile;

        public SmartBotBrain(BoardController boardController, BotProfileSO profile)
        {
            _boardController = boardController;
            _profile = profile;
        }

        public (UnitModel Unit, CellModel Destination) DecideMove(TeamColor team, int diceValue, BoardModel board)
        {
            List<UnitModel> allUnits = _boardController.GetAllUnitsByColor(team);
            
            UnitModel bestUnit = null;
            CellModel bestDestination = null;
            float maxScore = float.MinValue;

            foreach (var unit in allUnits)
            {
                if (!_boardController.CanInteract(unit, diceValue)) continue;
                if (unit.State == UnitState.Moving && _boardController.IsOvershootingGate(unit, diceValue)) continue; 

                if (_boardController.CanSpawnUnit(unit, diceValue))
                {
                    CellModel spawnCell = _boardController.GetSpawnCell(unit.TeamOwner);
                    float score = EvaluateState(unit, spawnCell, board);

                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestUnit = unit;
                        bestDestination = spawnCell;
                    }
                }
                else if (_boardController.CanMoveUnit(unit, diceValue))
                {
                    var destCells = _boardController.GetPotentialDestinationCell(unit, diceValue);
                    if (destCells == null || destCells.Count == 0) continue;
                    
                    foreach (var destCell in destCells)
                    {
                        float score = EvaluateState(unit, destCell, board);

                        if (score > maxScore)
                        {
                            maxScore = score;
                            bestUnit = unit;
                            bestDestination = destCell;
                        }
                    }
                }
            }

            return (bestUnit, bestDestination);
        }

        private float EvaluateState(UnitModel unit, CellModel targetCell, BoardModel board)
        {
            if (targetCell.Structure == CellStructure.Home)
                return _profile.WeightHome * unit.StepsMoved;
            
            float score = 0f;

            //Kick
            if (targetCell.HasUnit && targetCell.Unit.TeamOwner != unit.TeamOwner)
                score += _profile.WeightKick;

            //Safe
            if (targetCell.Structure == CellStructure.Spawn)
            {
                score += _profile.WeightSafe;
                score += 200f;
            }
                
            
            //Home
            if (targetCell.Structure == CellStructure.Gate)
                score += _profile.WeightHome;

            //Distance
            int steps = unit.StepsMoved + 1; 
            score += (_profile.WeightDistance * steps);

            //Danger
            float expectedDanger = CalculateExpectedDanger(unit.TeamOwner, targetCell, board);
            score -= expectedDanger;

            return score;
        }
        
        private float CalculateExpectedDanger(TeamColor myTeam, CellModel targetCell, BoardModel board)
        {
            //Các ô này miễn nhiễm sát thương
            if (targetCell.Structure == CellStructure.Spawn || 
                targetCell.Structure == CellStructure.Home)
            {
                return 0f;
            }

            float expectedDanger = 0f;
            
            //Lấy 6 ô phía sau lưng để kiểm tra địch
            var reversePath = _boardController.PathfindingService.GetReversePath(board, targetCell, 6);
            
            //Index 0 là ô hiện tại, từ 1 -> 6 là các ô phía sau
            for (int i = 1; i < reversePath.Count; i++)
            {
                CellModel cellBehind = reversePath[i];
                if (cellBehind.HasUnit && cellBehind.Unit.TeamOwner != myTeam)
                {
                    //Temp: Xác suất tung đúng mặt xúc xắc i là 1/6
                    //Chưa xử lý trường hợp ô gate
                    //đối thủ có thể ra diveValue = 1 hoặc 1 giá trị nằm 1 -> 6 thì xác suất là 2/6
                    float probability = 1f / 6f;
                    expectedDanger += (probability * _profile.WeightDanger);
                }
            }

            return expectedDanger;
        }
    }
}