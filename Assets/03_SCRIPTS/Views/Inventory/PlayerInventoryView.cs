using System;
using System.Collections.Generic;
using MADP.Models;
using MADP.Models.Inventory;
using MADP.Settings;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Views.Inventory
{
    public class PlayerInventoryView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private List<InventorySlotView> slots;
        
        private PlayerInventoryModel _currentModel;
        private bool _isOwnedByLocalPlayer;
        
        public Action<ItemDataSO, UnitModel> OnItemEquipRequested;
        
        private void Awake()
        {
            foreach (var slot in slots)
            {
                slot.OnItemDroppedOnUnit += HandleItemDropped;
            }
        }
        private void HandleItemDropped(ItemDataSO item, UnitView targetUnitView)
        {
            if (_currentModel == null) return;
            OnItemEquipRequested?.Invoke(item, targetUnitView.Model);
        }
        
        public void Setup(PlayerInventoryModel inventoryModel, bool isOwnedByLocalPlayer)
        {
            if (_currentModel != null)
            {
                _currentModel.OnInventoryUpdated -= RefreshUI;
            }

            _currentModel = inventoryModel;
            _isOwnedByLocalPlayer = isOwnedByLocalPlayer;

            if (_currentModel != null)
            {
                _currentModel.OnInventoryUpdated += RefreshUI;
                RefreshUI();
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void RefreshUI()
        {
            if (_currentModel == null) return;

            for (int i = 0; i < PlayerInventoryModel.ItemSlots; i++)
            {
                if (i < _currentModel.Items.Count)
                {
                    slots[i].Setup(_currentModel.Items[i], _isOwnedByLocalPlayer);
                }
                else
                {
                    slots[i].Setup(null);
                }
            }
        }

        public void Show() => panel.SetActive(true);
        public void Hide() => panel.SetActive(false);

        private void OnDestroy()
        {
            if (_currentModel != null)
            {
                _currentModel.OnInventoryUpdated -= RefreshUI;
            }
        }
    }
}