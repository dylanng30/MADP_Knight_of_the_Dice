using MADP.Models;
using MADP.Services.Combat.Interfaces;
using UnityEngine;

namespace MADP.Services.Combat
{
    public class CombatService : ICombatService
    {
        public CombatResult SimulateCombat(UnitModel attacker, UnitModel victim)
        {
            int rawDamage = attacker.Stat.Damage - victim.Stat.Armor;
            int finalDamage = Mathf.Max(0, rawDamage);
            bool isDead = (victim.Stat.CurrentHealth - finalDamage) <= 0;
                
            return new CombatResult
            {
                DamageDealt = finalDamage,
                IsVictimDead = isDead
            };
        }
        
    }
}