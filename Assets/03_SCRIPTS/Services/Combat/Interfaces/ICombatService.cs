using MADP.Models;

namespace MADP.Services.Combat.Interfaces
{
    public interface ICombatService
    {
        CombatResult SimulateCombat(UnitModel attacker, UnitModel victim);
    }
}