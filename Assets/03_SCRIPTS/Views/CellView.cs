using MADP.Models;
using UnityEngine;

namespace MADP.Views
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _unitAnchor;
        [SerializeField] private Transform _selectedSignal;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem _particleSystem;
        
        public Renderer Renderer => _renderer;
        public CellModel Model { get; private set; }

        public int Id;
        
        public void Setup(CellModel model)
        {
            Model = model;
            Id = Model.Index;
        }

        public Vector3 GetUnitPosition()
        {
            return _unitAnchor != null ?  _unitAnchor.position : transform.position;
        }

        public void SetHighlight(bool state)
        {
            _selectedSignal.gameObject.SetActive(state);
        }
    }
}