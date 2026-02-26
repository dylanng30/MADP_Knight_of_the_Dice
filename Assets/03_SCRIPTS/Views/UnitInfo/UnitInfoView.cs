using System;
using MADP.Models;
using MADP.Views.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.UnitInfo
{
    public class UnitInfoView : MonoBehaviour
    {
        [SerializeField] private Image Avatar;

        [Header("HEALTH")]
        [SerializeField] private Image healthBar;
        [SerializeField] private TextMeshProUGUI healthRatioTxt;

        [Header("DAMAGE")]
        [SerializeField] private TextMeshProUGUI damageTxt;

        [Header("ARMOR")]
        [SerializeField] private TextMeshProUGUI armorTxt;
        
        [Header("INVENTORY")]
        [SerializeField] private UnitInventoryView inventoryView;

        [Header("OTHERS")]
        [SerializeField] private Button hideButton;
        
        public Action HideAction;
        
        private UnitModel unitModel;
        

        private void Awake()
        {
            hideButton.onClick.AddListener((() => {HideAction?.Invoke();}));
        }

        public void Show(UnitModel model)
        {
            unitModel = model;
            UpdateUI();
            unitModel.OnStatsChanged += UpdateUI;
            
            if (inventoryView != null)
            {
                inventoryView.Initialize(unitModel.Inventory);
            }
            
            gameObject.SetActive(true);
        }

        public void Clear()
        {
            gameObject.SetActive(false);
            if(unitModel != null)
                unitModel.OnStatsChanged -= UpdateUI;
            unitModel = null;
        }

        public void UpdateUI()
        {
            healthBar.fillAmount = (float)unitModel.CurrentHealth / unitModel.MaxHealth;
            healthRatioTxt.text = $"{unitModel.CurrentHealth}/{unitModel.MaxHealth}";
            damageTxt.text = unitModel.Damage.ToString();
            armorTxt.text = unitModel.Armor.ToString();
        }

    }
}
