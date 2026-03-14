using MADP.Models;
using MADP.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _unitAnchor;
        [SerializeField] private Transform _hintSignal;
        [SerializeField] private Transform _selectionSignal;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem _particleSystem;

        [Header("OTHER SETTINGS")] 
        [SerializeField] private TextMeshProUGUI numberTxt;
        [SerializeField] private Image specialImg;
        
        public Renderer Renderer => _renderer;
        public CellModel Model { get; private set; }

        public CellStructure structure;
        public CellAttribute attribute;

        public int Id { get; private set; }
        
        public void Setup(CellModel model)
        {
            Model = model;
            Id = Model.Index;
            
            SetupStructure();
            SetupAttribute();

            Model.OnAttributeUpdated += SetupAttribute;
        }

        private void OnDestroy()
        {
            if (Model != null)
                Model.OnAttributeUpdated -= SetupAttribute;
        }

        public Vector3 GetUnitPosition()
        {
            return _unitAnchor != null ?  _unitAnchor.position : transform.position;
        }

        public void SetHighlightHint(bool state)
        {
            if (_hintSignal != null) 
                _hintSignal.gameObject.SetActive(state);
        }
        public void SetSelectionSignal(bool state)
        {
            if (_selectionSignal != null) 
                _selectionSignal.gameObject.SetActive(state);
        }

        private void SetupStructure()
        {
            structure = Model.Structure;
            numberTxt.gameObject.SetActive(false);
            if (structure == CellStructure.Home)
            {
                if (numberTxt) numberTxt.text = $"{Id + 1}";
                numberTxt.gameObject.SetActive(true);
            }
        }

        private void SetupAttribute()
        {
            attribute = Model.Attribute;
            switch (attribute)
            {
                case CellAttribute.Heal:
                    LoadIcon(Constants.HealIconPath);
                    break;
                case CellAttribute.Harm:
                    LoadIcon(Constants.HarmIconPath);
                    break;
                case CellAttribute.Gold:
                    LoadIcon(Constants.GoldIconPath);
                    break;
                case CellAttribute.Myth:
                    LoadIcon(Constants.MythIconPath);
                    break;
                default:
                    specialImg.gameObject.SetActive(false);
                    break;
            }
        }

        private void LoadIcon(string path)
        {
            specialImg.sprite = Resources.Load<Sprite>(path);
            specialImg.gameObject.SetActive(true);
        }
    }
}