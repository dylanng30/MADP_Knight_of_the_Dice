using MADP.Models;
using MADP.Services.Combat.Interfaces;
using UnityEngine;

namespace MADP.Services.Combat
{
    public class CombatService : ICombatService
    {
        public CombatResult SimulateCombat(UnitModel attacker, UnitModel victim)
        {
            int rawDamage = attacker.Damage - victim.Armor;
            int finalDamage = Mathf.Max(0, rawDamage);
            bool isDead = (victim.CurrentHealth - finalDamage) <= 0;
                
            return new CombatResult
            {
                DamageDealt = finalDamage,
                IsVictimDead = isDead
            };
        }
        
    }
}