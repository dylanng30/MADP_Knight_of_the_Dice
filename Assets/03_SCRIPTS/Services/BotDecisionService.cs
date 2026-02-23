using System.Collections.Generic;
using MADP.Controllers;
using MADP.Models;
using MADP.Services.AI.Interfaces;
using UnityEngine;

namespace MADP.Services
{
    public class BotDecisionService
    {
        private readonly Dictionary<TeamColor, IBotBrain> _bots = new ();
        public void RegisterBotStrategy(TeamColor team, IBotBrain botBrain)
        {
            Debug.Log($"Team {team} dùng {botBrain.GetType()}");
            _bots[team] = botBrain;
        }
        
        public (UnitModel Unit, CellModel Destination) GetBestMove(TeamColor team, int diceValue, BoardModel board)
        {
            if (_bots.TryGetValue(team, out var strategy))
            {
                return strategy.DecideMove(team, diceValue, board);
            }
            
            Debug.LogError($"Chưa đăng ký AI cho team {team}");
            return (null, null);
        }
    }
}