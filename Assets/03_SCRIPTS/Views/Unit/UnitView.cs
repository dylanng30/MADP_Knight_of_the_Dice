using System.Collections;
using System.Collections.Generic;
using MADP.Models;
using MADP.Settings;
using TMPro;
using UnityEngine;

namespace MADP.Views.Unit
{
    public class UnitView : MonoBehaviour
    {
        [Header("---MATERIALS---")] public List<Renderer> PrimarySign;
        public List<Renderer> SecondarySign;
        public List<Renderer> TertiarySign;

        [Header("---COMPONENTS---")] [SerializeField]
        private Collider collider;

        [SerializeField] private Animator animator;
        public Collider Collider => collider;
        public UnitModel Model { get; private set; }

        public RoleType Role;
        public int Level;
        public int Health;
        public int Damage;
        public int Armor;
        
        public void Setup(UnitModel model)
        {
            Model = model;

            Role = model.RoleType;
            Level = Model.Id;
            Health = Model.Stat.CurrentHealth;
            Damage = Model.Stat.Damage;
            Armor = Model.Stat.Armor;
        }

        public void PlayAnimation(string animationName)
        {
            // Debug.Log("Playing animation: " + animationName); 
            //animator.Play(animationName);
        }

        public void Spawn(Vector3 position)
        {
            transform.position = position;
        }

        public IEnumerator MoveTo(Vector3 targetPosition)
        {
            if (!this || !gameObject)
                yield break;

            Vector3 moveDirection = targetPosition - transform.position;
            Rotate(moveDirection);

            while (true)
            {
                if (!this || !gameObject)
                    yield break;

                if (!isActiveAndEnabled)
                    yield break;

                if (Vector3.Distance(transform.position, targetPosition) <= 0.05f)
                    break;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    10f
                );

                yield return null;
            }
        }

        public void OnAnimationStarted()
        {
        }

        public void OnAnimationTriggerCalled()
        {
        }

        public void OnAnimationFinished()
        {
        }

        public void Rotate(Vector3 direction)
        {
            direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }
        }
    }
}