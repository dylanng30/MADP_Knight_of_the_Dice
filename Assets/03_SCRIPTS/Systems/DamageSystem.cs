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
            ActionSystem.AttachPerformer<AttackUA>(DealDamagePerformer);
        }

        private void OnDisable()
        {
            ActionSystem.DetachPerformer<AttackUA>();
        }

        private IEnumerator DealDamagePerformer(AttackUA attackUA)
        {
            attackUA.AttackerView.PlayAnimation("Aattack");
            attackUA.VictimView.PlayAnimation("Hit");
            yield return null;
        }
    }
}