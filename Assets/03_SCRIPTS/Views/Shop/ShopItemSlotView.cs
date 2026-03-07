using System;
using MADP.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MADP.Views.Shop
{
    public class ShopItemSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;
        
        [Header("States")]
        [SerializeField] private GameObject purchasedOverlay;
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color expensiveColor = Color.red;
        
        private bool _isPurchased = false;
        
        public ItemDataSO CurrentItem { get; private set; }
        public Action<ItemDataSO> OnBuyClicked;
        
        private void Awake()
        {
            buyButton.onClick.AddListener(() => 
            {
                if (!_isPurchased) OnBuyClicked?.Invoke(CurrentItem);
            });
        }

        public void Setup(ItemDataSO item)
        {
            CurrentItem = item;
            iconImage.sprite = item.Icon;
            priceText.text = item.Price.ToString();
            
            _isPurchased = false;
            if (purchasedOverlay != null) purchasedOverlay.SetActive(false);
            //nameText.text = item.ItemName;
            //statText.text = $"HP: {item.BonusHealth} / DMG: {item.BonusDamage} / DEF: {item.BonusArmor}";
        }
        public void UpdateAffordability(int currentGold)
        {
            if (_isPurchased) 
                return;
            
            bool isAffordable = currentGold >= CurrentItem.Price;
            priceText.color = isAffordable ? affordableColor : expensiveColor;
            buyButton.interactable = isAffordable;
        }
        
        public void SetPurchased()
        {
            _isPurchased = true;
            buyButton.interactable = false;
            if (purchasedOverlay != null) purchasedOverlay.SetActive(true);
        }

        //Hover show stats
        public void OnPointerEnter(PointerEventData eventData)
        {
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            
        }
    }
}