using System.Collections.Generic;
using MADP.Models.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Inventory
{
    public class UnitInventoryView : MonoBehaviour
    {
        [SerializeField] private List<Image> _slots;
        
        private UnitInventoryModel _inventory;

        public void Initialize(UnitInventoryModel inventory)
        {
            _inventory = inventory;
            _inventory.OnInventoryUpdated += RefreshUI;
            
            RefreshUI();
        }
        
        private void RefreshUI()
        {
            var items = _inventory.Items;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (i < items.Count)
                {
                    _slots[i].sprite = items[i].Icon;
                    _slots[i].enabled = true;
                }
                else
                {
                    _slots[i].enabled = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (_inventory != null)
                _inventory.OnInventoryUpdated -= RefreshUI;
        }
    }
}