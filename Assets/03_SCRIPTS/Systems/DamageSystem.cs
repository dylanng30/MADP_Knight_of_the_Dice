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
            var attacker = attackUA.AttackerView;
            var victim = attackUA.VictimView;
            
            var attackerOriginRotation = attacker.transform.rotation;
            var victimOriginRotation = victim.transform.rotation;
            
            Vector3 faceDirection = victim.transform.position - attacker.transform.position;
            faceDirection.y = 0;
            
            attacker.transform.rotation = Quaternion.LookRotation(faceDirection);
            victim.transform.rotation = Quaternion.LookRotation(-faceDirection);
            
            attacker.PlayAnimation("Attack");
            //victim.PlayAnimation("Attack");
            
            yield return new WaitForSeconds(2f);
            attackUA.OnHit?.Invoke();
            
            if (attackUA.IsDead)
            {
                victim.PlayAnimation("Death");
                yield return new WaitForSeconds(2f); 
                attackUA.OnDeathAnimationFinished?.Invoke();
            }
            else
            {
                victim.transform.rotation = victimOriginRotation;
            }
        }
    }
}