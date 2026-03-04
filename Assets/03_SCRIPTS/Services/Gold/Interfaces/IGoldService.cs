using System;
using System.Collections.Generic;
using MADP.Models;

namespace MADP.Services.Gold.Interfaces
{
    public interface IGoldService
    {
        void Initialize(int initialGold, List<LobbySlotModel> activePlayers);
        event Action<TeamColor, int> OnGoldChanged;
        
        int GetGold(TeamColor team);
        void AddGold(TeamColor team, int amount);
        bool TrySpendGold(TeamColor team, int amount);
        
        void ApplyRoundBonus();
        void ApplyStuckBonus(TeamColor team);
        void HandleUnitDeath(UnitModel victim, UnitModel killer);
    }
}