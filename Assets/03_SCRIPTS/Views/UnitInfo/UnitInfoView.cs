using System;
using MADP.Models;
using MADP.Settings;
using MADP.Views.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.UnitInfo
{
    public class UnitInfoView : MonoBehaviour
    {
        [SerializeField] private Image avatar;
        [SerializeField] private Image teamColor;

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
        private TeamColorDatabaseSO teamColorDB;
        

        private void Awake()
        {
            hideButton.onClick.AddListener((() => {HideAction?.Invoke();}));
        }

        public void Show(UnitModel model, Sprite avatarSprite)
        {
            unitModel = model;
            
            if (avatar != null) 
                avatar.sprite = avatarSprite;
            
            UpdateTeamColor();
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

        private void UpdateTeamColor()
        {
            if(teamColorDB == null)
                teamColorDB = Resources.Load<TeamColorDatabaseSO>("TeamColorDB");
            teamColor.color = teamColorDB.GetTeamColor(unitModel.TeamOwner, Priority.Primary);
        }

    }
}
