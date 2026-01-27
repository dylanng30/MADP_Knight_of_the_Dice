using System;
using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Utilities;
using UnityEngine;

namespace MADP.Controllers
{
    public class GoldController : Singleton<GoldController>
    {
        private Dictionary<TeamColor, int> _goldMapper = new();

        public event Action<int> OnGoldChanged;

        private TeamColor _playerTeamColor = TeamColor.None;

        //Config
        private const int GOLD_PER_ROUND = 1;
        private const int GOLD_WHEN_STUCK = 1;
        private const int GOLD_WHEN_DIE = 1;

        public void RegisterPlayerColorTeam(TeamColor playerTeamColor)
        {
            _playerTeamColor = playerTeamColor;
            Initialize();

        }
        private void Initialize()
        {
            _goldMapper.Clear();
            _goldMapper.Add(TeamColor.Red, 10);
            _goldMapper.Add(TeamColor.Yellow, 10);
            _goldMapper.Add(TeamColor.Blue, 10);
            _goldMapper.Add(TeamColor.Green, 10);

            NotifyPlayersGoldChanged();
        }

        public bool TrySpendGold(TeamColor team, int amount)
        {
            if (!_goldMapper.ContainsKey(team)) return false;
            if (_goldMapper[team] < amount) return false;

            _goldMapper[team] -= amount;
            NotifyPlayersGoldChanged();
            Debug.Log($"Team {team} spent {amount} gold. Remaining: {_goldMapper[team]}");
            return true;
        }

        public void AddGold(TeamColor teamColor, int amount)
        {
            if (!_goldMapper.ContainsKey(teamColor))
                return;

            _goldMapper[teamColor] += amount;
            NotifyPlayersGoldChanged();
        }

        public int GetGold(TeamColor team) => _goldMapper.ContainsKey(team) ? _goldMapper[team] : 0;

        public void ApplyRoundBonus()
        {
            foreach (var team in _goldMapper.Keys.ToList())
            {
                AddGold(team, GOLD_PER_ROUND);
            }
            Debug.Log("Applied Round Bonus Gold");
        }

        public void ApplyStuckBonus(TeamColor team)
        {
            AddGold(team, GOLD_WHEN_STUCK);
            Debug.Log($"Applied Stuck Bonus for {team}");
        }

        public void HandleUnitDeath(UnitModel victim, UnitModel killer)
        {
            AddGold(victim.TeamOwner, GOLD_WHEN_DIE);

            if (killer != null && killer.TeamOwner != victim.TeamOwner)
            {
                int reward = Mathf.CeilToInt(victim.Cost / 2f);
                AddGold(killer.TeamOwner, reward);
            }
        }

        private void NotifyPlayersGoldChanged()
        {
            OnGoldChanged?.Invoke(_goldMapper[_playerTeamColor]);
        }

    }
}
