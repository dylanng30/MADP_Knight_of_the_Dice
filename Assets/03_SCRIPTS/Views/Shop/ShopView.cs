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
        
        public Action<ItemDataSO> OnItemPurchaseRequested;
        private List<ShopItemSlotView> _spawnedSlots = new ();

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
        public void RefreshAffordability(int currentGold)
        {
            foreach (var slot in _spawnedSlots)
            {
                slot.UpdateAffordability(currentGold);
            }
        }
        
        public void MarkItemAsPurchased(ItemDataSO item)
        {
            foreach (var slot in _spawnedSlots)
            {
                if (slot.CurrentItem == item)
                {
                    slot.SetPurchased();
                    break;
                }
            }
        }

        // Lấy vùng RectTransform của một ô chứa vật phẩm dựa trên chỉ số (index).
        public RectTransform GetItemSlotRect(int index)
        {
            if (index >= 0 && index < _spawnedSlots.Count)
            {
                return _spawnedSlots[index].GetComponent<RectTransform>();
            }
            return null;
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