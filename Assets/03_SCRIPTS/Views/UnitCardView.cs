using System;
using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views
{
    public class UnitCardView : MonoBehaviour
    {
        [Header("---STATS---")] 
        [SerializeField] private TextMeshProUGUI nameTxt;
        [SerializeField] private TextMeshProUGUI hpTxt;
        [SerializeField] private TextMeshProUGUI dmgTxt;
        [SerializeField] private TextMeshProUGUI armorTxt;
        [SerializeField] private TextMeshProUGUI costTxt;

        [Header("---OTHERS---")] 
        [SerializeField] private Image avatarImg;
        [SerializeField] private Button actionButton;
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color expensiveColor = Color.red;

        private UnitModel _model;
        private Action<UnitModel> _onClicked;

        public void Setup(UnitModel model, Sprite avatar, Action<UnitModel> onClicked)
        {
            _model = model;
            _onClicked = onClicked;
            
            if(avatarImg != null) avatarImg.sprite = avatar;
            if (nameTxt) nameTxt.text = $"Unit {_model.Id + 1}";
            
            UpdateStats();
            
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => _onClicked?.Invoke(_model));
            
            _model.OnStatsChanged += UpdateStats;
        }
        
        private void OnDestroy()
        {
            if (_model != null) _model.OnStatsChanged -= UpdateStats;
        }

        private void UpdateStats()
        {
            if (hpTxt) hpTxt.text = _model.MaxHealth.ToString();
            if (dmgTxt) dmgTxt.text = _model.Damage.ToString();
            if (armorTxt) armorTxt.text = _model.Armor.ToString();
            if (costTxt) costTxt.text = $"{_model.Cost}$";
        }

        public void SetInteractable(bool canSpawn, int currentGold)
        {
            bool isAffordable = currentGold >= _model.Cost;
            actionButton.interactable = canSpawn;
            if (costTxt) costTxt.color = isAffordable ? affordableColor : expensiveColor;
        }
    }
}