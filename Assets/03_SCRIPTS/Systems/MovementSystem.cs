using System;
using System.Collections;
using MADP.Models.UnitActions;
using MADP.Views;
using UnityEngine;

namespace MADP.Systems
{
    public class MovementSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            ActionSystem.AttachPerformer<MoveUA>(MovePerformer);
        }

        private void OnDisable()
        {
            ActionSystem.DetachPerformer<MoveUA>();
        }

        private IEnumerator MovePerformer(MoveUA moveUA)
        {
            Debug.Log("[MOVEMENT SYSTEM] Move performer");
            var unitView = moveUA.UnitView;
            
            //Move
            for (int i = 0; i < moveUA.Path.Count; i++)
            {
                Vector3 targetPosition = moveUA.Path[i];
                yield return unitView.MoveTo(targetPosition);
            }
            
            //Kiểm tra đích đến 
            //Nếu có enemy thì tấn công
            //DealDamageUA dealDamageUA = new DealDamageUA(unitModel, unitModel.DMG);
            //ActionSystem.Instance.AddReaction(dealDamageUA);
        }
    }
}