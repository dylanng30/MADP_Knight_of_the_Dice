using MADP.Models;
using TMPro;
using UnityEngine;

namespace MADP.Views
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        [SerializeField] private Collider collider;
        public Renderer Renderer => renderer;
        public Collider Collider => collider;
        
        public UnitModel Model { get; private set; }

        public void Setup(UnitModel model)
        {
            Model = model;
        }

        public void MoveToPosition(Vector3 position)
        {
            //Temp
            transform.position = position;
        }
    }
}