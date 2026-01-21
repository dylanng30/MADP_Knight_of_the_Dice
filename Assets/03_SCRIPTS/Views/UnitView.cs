using MADP.Models;
using UnityEngine;

namespace MADP.Views
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        public Renderer Renderer => renderer;
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