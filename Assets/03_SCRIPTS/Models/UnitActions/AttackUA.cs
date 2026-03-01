using System;
using System.Collections.Generic;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Models.UnitActions
{
    public class AttackUA : BaseUnitAction
    {
        public UnitView AttackerView { get; private set; }
        public UnitView VictimView { get; private set; }
        public bool IsDead { get; private set; }
        
        public Action OnHit { get; private set; }
        public Action OnDeathAnimationFinished { get; private set; }
        
        public AttackUA(UnitView attacker, UnitView victim, bool isDead = false, Action onHit = null, Action onDeathAnimationFinished = null)
        {
            AttackerView = attacker;
            VictimView = victim;
            IsDead = isDead;
            OnHit = onHit;
            OnDeathAnimationFinished = onDeathAnimationFinished;
        }
    }
}