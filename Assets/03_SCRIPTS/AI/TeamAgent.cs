using System.Collections.Generic;
using System.Linq;
using MADP.Controllers;
using MADP.Models;
using MADP.Services;
using MADP.Services.Gold;
using MADP.Services.Gold.Interfaces;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace _03_SCRIPTS.AI
{
    public class TeamAgent : Agent
    {
        [SerializeField] private BoardController boardController;
        [SerializeField] private TurnController turnController;
        [SerializeField] private TeamColor agentColor;
        public TeamColor AgentColor => agentColor;
        [SerializeField] public int maxAgentTurn = 30;
        [SerializeField] private int maxGold = 50;
        [SerializeField] private int maxCell = 56;
        [SerializeField] private int maxStat = 5;

        private IGoldService GoldService => boardController.GoldService;      
        private readonly FindNearestUnitService _findNearestUnit = new();
        private List<UnitModel> _units;
        private int _diceValue;
        private int _goldValue;
        public int TurnCounter { get; set; }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            turnController.ResetEnvironment();
            TurnCounter = 0;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            base.CollectObservations(sensor);
            
            _units = boardController.GetAllUnitsByColor(agentColor).OrderBy(u => u.Id).ToList();
            _diceValue = turnController.CurrentDiceValue;
            _goldValue = GoldService.GetGold(agentColor);

            // Thu thập thông tin theo từng unit
            foreach (var unit in _units)
            {
                // Thu thập thông tin ô của unit đang đứng
                var cell = boardController.GetCurrentCellOfUnit(unit);
                var isInNest = (cell == null) ? 1 : 0;
                var cellNormalIndex = (cell != null && cell.Structure == CellStructure.Normal) ? cell.Index : 0;
                var normalizedCellNormalIndex = (float)cellNormalIndex / maxCell;
                var isInGate = (cell != null && cell.Structure == CellStructure.Gate) ? 1 : 0;
                var cellHomeIndex = (cell != null && cell.Structure == CellStructure.Home) ? cell.Index : 0;
                var normalizedCellHomeIndex = cellHomeIndex / 6f;

                // Thu thập thông tin kẻ thù gần nhất phía trước
                var nearestForwardEnemy = _findNearestUnit.FindNearestEnemyForward(boardController.Board.AroundCells,
                    cellNormalIndex, agentColor);

                var normalizedForwardHealth =
                    (nearestForwardEnemy.Unit != null)
                        ? (float)nearestForwardEnemy.Unit.Stat.CurrentHealth / maxStat
                        : 0;
                var normalizedForwardDamage =
                    (nearestForwardEnemy.Unit != null) ? (float)nearestForwardEnemy.Unit.Stat.Damage / maxStat : 0;
                var normalizedForwardArmor =
                    (nearestForwardEnemy.Unit != null) ? (float)nearestForwardEnemy.Unit.Stat.Armor / maxStat : 0;
                var normalizedDistanceToForward = (float)nearestForwardEnemy.Distance / maxCell;

                // Thu thập thông tin về kẻ thù gần nhất phía sau
                var nearestBackwardEnemy = _findNearestUnit.FindNearestEnemyBackward(boardController.Board.AroundCells,
                    cellNormalIndex, agentColor);
                var normalizedBackwardHealth =
                    (nearestBackwardEnemy.Unit != null)
                        ? (float)nearestBackwardEnemy.Unit.Stat.CurrentHealth / maxStat
                        : 0;
                var normalizedBackwardDamage =
                    (nearestBackwardEnemy.Unit != null) ? (float)nearestBackwardEnemy.Unit.Stat.Damage / maxStat : 0;
                var normalizedBackwardArmor =
                    (nearestBackwardEnemy.Unit != null) ? (float)nearestBackwardEnemy.Unit.Stat.Armor / maxStat : 0;
                var normalizedDistanceToBackward = (float)nearestBackwardEnemy.Distance / maxCell;

                // Thu thập thông tin cá nhân
                var canSpawn = (unit.Cost <= _goldValue) ? 1 : 0;
                var canAttack =
                    (_diceValue == nearestForwardEnemy.Distance || _diceValue == nearestBackwardEnemy.Distance) ? 1 : 0;
                var nearestUnit =
                    _findNearestUnit.FindNearestUnitForward(boardController.Board.AroundCells, cellNormalIndex);
                var canMove = (nearestUnit.Unit != null && nearestUnit.Distance > _diceValue) ? 1 : 0;
                var canEnterHome = (cell != null && cell.Structure == CellStructure.Gate) ? 1 : 0;
                var canBeKilledNextTurn =
                    (nearestForwardEnemy.Distance <= 6 || nearestBackwardEnemy.Distance <= 6) ? 1 : 0;

                sensor.AddObservation(isInNest);
                sensor.AddObservation(normalizedCellNormalIndex);
                sensor.AddObservation(isInGate);
                sensor.AddObservation(normalizedCellHomeIndex);

                sensor.AddObservation(normalizedForwardHealth);
                sensor.AddObservation(normalizedForwardDamage);
                sensor.AddObservation(normalizedForwardArmor);
                sensor.AddObservation(normalizedDistanceToForward);

                sensor.AddObservation(normalizedBackwardHealth);
                sensor.AddObservation(normalizedBackwardDamage);
                sensor.AddObservation(normalizedBackwardArmor);
                sensor.AddObservation(normalizedDistanceToBackward);

                sensor.AddObservation(canSpawn);
                sensor.AddObservation(canAttack);
                sensor.AddObservation(canMove);
                sensor.AddObservation(canBeKilledNextTurn);
                sensor.AddObservation(canEnterHome);
            }

            // Thu thập thông tin bao nhiêu unit đã về nhà và trên sân
            var inHomeCell = 0;
            var inNormalCell = 0;
            foreach (var unit in _units)
            {
                var cell = boardController.GetCurrentCellOfUnit(unit);
                if (cell != null && cell.Structure == CellStructure.Home)
                {
                    inHomeCell += 1;
                }

                if (cell != null && cell.Structure == CellStructure.Normal)
                {
                    inNormalCell += 1;
                }
            }

            var normalizedInHomeCell = inHomeCell / 4f;
            var normalizedInNormalCell = inNormalCell / 4f;
            var normalizedDiceValue = _diceValue / 6f;
            var normalizedGoldValue = (float)_goldValue / maxGold;
            var normalizedTurn = (float)TurnCounter / maxAgentTurn;

            sensor.AddObservation(normalizedInHomeCell);
            sensor.AddObservation(normalizedInNormalCell);
            sensor.AddObservation(normalizedDiceValue);
            sensor.AddObservation(normalizedGoldValue);
            sensor.AddObservation(normalizedTurn);
            
            sensor.AddObservation(agentColor == TeamColor.Red ? 1 : 0);
            sensor.AddObservation(agentColor == TeamColor.Yellow ? 1 : 0);
            sensor.AddObservation(agentColor == TeamColor.Blue ? 1 : 0);
            sensor.AddObservation(agentColor == TeamColor.Green ? 1 : 0);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);

            PlayerAction(actions.DiscreteActions);
        }

        void PlayerAction(ActionSegment<int> act)
        {
            int unitIdx = act[0];
            int playerAction = act[1];

            switch (playerAction)
            {
                case 0:
                    boardController.SpawnOnly(_units[unitIdx], _diceValue, turnController.EndTurn);
                    break;
                case 1:
                    boardController.MoveForwardOnly(_units[unitIdx], _diceValue, turnController.EndTurn);
                    break;
                case 2:
                    boardController.AttackForwardOnly(_units[unitIdx], _diceValue, turnController.EndTurn);
                    break;
                case 3:
                    boardController.AttackBackwardOnly(_units[unitIdx], _diceValue, turnController.EndTurn);
                    break;
                case 4:
                    boardController.MoveToHomeOnly(_units[unitIdx], _diceValue, turnController.EndTurn);
                    break;
                case 5:
                    boardController.MoveInsideHomeOnly(_units[unitIdx], _diceValue, turnController.EndTurn);
                    break;
            }
        }

        // public override void Heuristic(in ActionBuffers actionsOut)
        // {
        //     base.Heuristic(in actionsOut);
        //
        //     var discrete = actionsOut.DiscreteActions;
        //
        //     if (Input.GetKeyDown(KeyCode.Alpha1))
        //     {
        //         discrete[0] = 0;
        //         discrete[1] = 0;
        //     }
        //
        //     if (Input.GetKeyDown(KeyCode.Alpha2))
        //     {
        //         discrete[0] = 0;
        //         discrete[1] = 1;
        //     }
        // }
    }
}