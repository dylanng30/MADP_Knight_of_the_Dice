using System;
using MADP.Models;
using MADP.Services.Gold.Interfaces;

namespace MADP.Services.Gold
{
    public class GoldService : IGoldService
    {
        private readonly GoldModel _goldModel;
        
        public GoldService()
        {
            _goldModel = new GoldModel();
            _goldModel.OnTeamGoldChanged += HandleGoldChanged;
        }

        public void Initialize(int initialGold)
        {
            var teams = new[] { TeamColor.Red, TeamColor.Yellow, TeamColor.Blue, TeamColor.Green };
            _goldModel.Initialize(teams, initialGold);
        }
        
        public event Action<TeamColor, int> OnGoldChanged;
        
        public int GetGold(TeamColor team) => _goldModel.GetGoldByTeam(team);

        public void AddGold(TeamColor team, int amount)
        {
            _goldModel.AddGold(team, amount);
        }

        public bool TrySpendGold(TeamColor team, int amount)
        {
            return _goldModel.TrySpendGold(team, amount);
        }

        public void ApplyRoundBonus()
        {
            _goldModel.ApplyRoundBonus();
        }

        public void ApplyStuckBonus(TeamColor team)
        {
            _goldModel.ApplyStuckBonus(team);
        }

        public void HandleUnitDeath(UnitModel victim, UnitModel killer)
        {
            _goldModel.HandleUnitDeath(victim, killer);
        }

        #region ---HELPERS ---
        private void HandleGoldChanged(TeamColor team, int totalGold)
        {
            OnGoldChanged?.Invoke(team, _goldModel.GetGoldByTeam(team));
        }
        #endregion
        
    }
}