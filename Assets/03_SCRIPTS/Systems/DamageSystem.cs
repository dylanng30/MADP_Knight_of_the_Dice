using System;
using System.Collections;
using MADP.Models.UnitActions;
using UnityEngine;

namespace MADP.Systems
{
    public class DamageSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            ActionSystem.AttachPerformer<DealDamageUA>(DealDamagePerformer);
        }

        private void OnDisable()
        {
            ActionSystem.DetachPerformer<DealDamageUA>();
        }

        private IEnumerator DealDamagePerformer(DealDamageUA dealDamageUA)
        {
            int dmgAmount = dealDamageUA.Amount;
            var target = dealDamageUA.TargetUnitModel;
            var targetStatModel = dealDamageUA.TargetStatModel;
            targetStatModel.TakeDamage(dmgAmount);
            //target.TakeDamage(dmgAmount);
            yield return null;
        }
    }
}