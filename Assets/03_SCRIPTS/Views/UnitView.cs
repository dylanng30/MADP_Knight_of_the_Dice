using System.Collections;
using MADP.Models;
using TMPro;
using UnityEngine;

namespace MADP.Views
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        [SerializeField] private Collider collider;
        [SerializeField] private Animator animator;
        public Renderer Renderer => renderer;
        public Collider Collider => collider;
        
        public UnitModel Model { get; private set; }

        public void Setup(UnitModel model)
        {
            Model = model;
        }

        public void PlayAnimation(string animationName)
        {
            Debug.Log("Playing animation: " + animationName);
            //animator?.Play(animationName);
        }

        public void MoveToPosition(Vector3 position)
        {
            //Temp
            transform.position = position;
        }

        public IEnumerator MoveTo(Vector3 targetPosition)
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.1f);
                yield return null;
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
}