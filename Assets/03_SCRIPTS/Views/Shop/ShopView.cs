using System;
using System.Collections.Generic;
using MADP.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Shop
{
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private Transform itemContainer;
        [SerializeField] private ShopItemSlotView shopItemSlotPrefab;

        [SerializeField] private Button exitButton;
        
        public Action<ItemDataSO> OnItemPurchaseRequested;
        public Action OnCloseRequested;
        
        private List<ShopItemSlotView> _spawnedSlots = new ();
        
        private void Awake()
        {
            exitButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
        }
        
        public void Setup(List<ItemDataSO> items)
        {
            foreach (var slot in _spawnedSlots) 
                Destroy(slot.gameObject);
            _spawnedSlots.Clear();
            
            foreach (var item in items)
            {
                var slotView = Instantiate(shopItemSlotPrefab, itemContainer);
                slotView.Setup(item);
                slotView.OnBuyClicked += (selectedItem) => OnItemPurchaseRequested?.Invoke(selectedItem);
                
                _spawnedSlots.Add(slotView);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}