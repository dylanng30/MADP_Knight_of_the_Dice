using System;
using MADP.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Shop
{
    public class ShopItemSlotView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI statText;
        
        [SerializeField] private Button buyButton;
        
        private ItemDataSO _currentItem;
        public Action<ItemDataSO> OnBuyClicked;

        private void Awake()
        {
            buyButton.onClick.AddListener((() => {OnBuyClicked?.Invoke(_currentItem);}));
        }

        public void Setup(ItemDataSO item)
        {
            _currentItem = item;
            iconImage.sprite = item.Icon;
            nameText.text = item.ItemName;
            statText.text = $"HP: {item.BonusHealth} / DMG: {item.BonusDamage} / DEF: {item.BonusArmor}";
        }
        }
}