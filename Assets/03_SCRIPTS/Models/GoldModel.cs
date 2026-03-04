using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace MADP.Models
{
    public class GoldModel
    {
        private Dictionary<TeamColor, int> _goldByTeam = new ();
        public event Action<TeamColor, int> OnTeamGoldChanged;

        private readonly int _goldPerRound;
        private readonly int _goldWhenStuck;
        private readonly int _goldWhenDie;

        public GoldModel(int goldPerRound = 1, int goldWhenStuck = 1, int goldWhenDie = 1)
        {
            _goldPerRound = goldPerRound;
            _goldWhenStuck = goldWhenStuck;
            _goldWhenDie = goldWhenDie;
        }

        public void Initialize(IEnumerable<TeamColor> teamColors, int initialGold = 10)
        {
            _goldByTeam.Clear();

            foreach (var team in teamColors)
            {
                _goldByTeam[team] = initialGold;
                OnTeamGoldChanged?.Invoke(team, initialGold);
            }
        }

        public bool TrySpendGold(TeamColor team, int amount)
        {
            if(!_goldByTeam.ContainsKey(team)) return false;
            if(_goldByTeam[team] < amount) return false;
            
            _goldByTeam[team] -= amount;
            OnTeamGoldChanged?.Invoke(team, _goldByTeam[team]);
            return true;
        }

        public void AddGold(TeamColor team, int amount)
        {
            if(!_goldByTeam.ContainsKey(team)) return;
            
            _goldByTeam[team] += amount;
            OnTeamGoldChanged?.Invoke(team, _goldByTeam[team]);
        }
        
        public int GetGoldByTeam(TeamColor team) => _goldByTeam.ContainsKey(team) ? _goldByTeam[team] : 0;

        public void ApplyRoundBonus()
        {
            foreach (var team in _goldByTeam.Keys.ToList())
            {
                AddGold(team, _goldPerRound);
            }
        }

        public void ApplyStuckBonus(TeamColor team)
        {
            AddGold(team, _goldWhenStuck);
        }

        public void HandleUnitDeath(UnitModel victim, UnitModel attacker)
        {
            AddGold(victim.TeamOwner, _goldWhenDie);

            if (attacker != null && attacker.TeamOwner != victim.TeamOwner)
            {
                int reward = Mathf.CeilToInt(victim.Cost / 2f);
                AddGold(attacker.TeamOwner, reward);
            }
        }
    }
}