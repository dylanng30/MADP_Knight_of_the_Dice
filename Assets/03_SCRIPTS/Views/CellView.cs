using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _unitAnchor;
        [SerializeField] private Transform _selectedSignal;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem _particleSystem;

        [Header("OTHER SETTINGS")] 
        [SerializeField] private TextMeshProUGUI numberTxt;
        [SerializeField] private Image specialImg;
        
        public Renderer Renderer => _renderer;
        public CellModel Model { get; private set; }

        public int Id { get; private set; }
        
        public void Setup(CellModel model)
        {
            Model = model;
            Id = Model.Index;
            
            SetupStructure();
            SetupAttribute();
        }

        public Vector3 GetUnitPosition()
        {
            return _unitAnchor != null ?  _unitAnchor.position : transform.position;
        }

        public void SetHighlight(bool state)
        {
            _selectedSignal.gameObject.SetActive(state);
        }

        private void SetupStructure()
        {
            var structure = Model.Structure;
            numberTxt.gameObject.SetActive(false);
            if (structure == CellStructure.Home)
            {
                if (numberTxt) numberTxt.text = $"{Id + 1}";
                numberTxt.gameObject.SetActive(true);
            }
        }

        private void SetupAttribute()
        {
            var attribute = Model.Attribute;
            specialImg.gameObject.SetActive(false);
        }
    }
}