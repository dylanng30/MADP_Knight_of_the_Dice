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
            var unitView = moveUA.UnitView;
            unitView.PlayAnimation("Move");
            for (int i = 0; i < moveUA.Path.Count; i++)
            {
                Vector3 targetPosition = moveUA.Path[i];
                yield return unitView.MoveTo(targetPosition);
            }
            
            if (moveUA.DefaultDirection != Vector3.zero)
                unitView.Rotate(moveUA.DefaultDirection);
            
            unitView.PlayAnimation("Idle");
        }
    }
}