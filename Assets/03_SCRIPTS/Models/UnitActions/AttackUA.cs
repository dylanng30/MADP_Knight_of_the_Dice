using System.Collections.Generic;
using MADP.Views;
using UnityEngine;

namespace MADP.Models.UnitActions
{
    public class AttackUA : BaseUnitAction
    {
        public UnitView AttackerView { get; private set; }
        public UnitView VictimView { get; private set; }
        public bool IsDead { get; private set; }
        
        public AttackUA(UnitView attacker, UnitView victim, bool isDead = false)
        {
            AttackerView = attacker;
            VictimView = victim;
            IsDead = isDead;
        }
    }
}