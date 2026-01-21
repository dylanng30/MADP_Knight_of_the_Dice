using MADP.Models;
using UnityEngine;

namespace MADP.Views
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _unitAnchor;
        public Renderer Renderer => _renderer;
        public CellModel Model { get; private set; }
        
        public void Setup(CellModel model)
        {
            Model = model;
        }

        public Vector3 GetUnitPosition()
        {
            return _unitAnchor != null ?  _unitAnchor.position : transform.position;
        }
    }
}