using System.Collections;
using MADP.Models;
using UnityEngine;

namespace MADP.Views
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        public UnitModel Model;
        
        public IEnumerator MoveTo(Vector3 targetPosition)
        {
            PlayAnimation("Move");
            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.05f);
                yield return null;
            }
            
            transform.position = targetPosition;
            yield return new WaitForSeconds(0.5f);
        }

        private void PlayAnimation(string animationName)
        {
            if(animator == null)
                return;
            
            animator.Play(animationName);
        }
    }
}

