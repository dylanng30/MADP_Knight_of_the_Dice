using System;
using System.Collections.Generic;
using MADP.Settings;
using UnityEngine;

namespace MADP.Models.Inventory
{
    public class UnitInventoryModel
    {
        public const int MAX_SLOTS = 3;
        
        private readonly List<ItemDataSO> _items = new ();
        
        public event Action OnInventoryUpdated;
        public List<ItemDataSO> Items => _items;
        public bool IsFull => _items.Count >= MAX_SLOTS;

        public bool AddItem(ItemDataSO item)
        {
            if (IsFull)
            {
                Debug.LogWarning("Inventory is full!");
                return false;
            }

            _items.Add(item);
            OnInventoryUpdated?.Invoke();
            return true;
        }

        public bool RemoveItem(ItemDataSO item)
        {
            if (_items.Contains(item))
            {
                _items.Remove(item);
                OnInventoryUpdated?.Invoke();
                return true;
            }
            return false;
        }
        
        public int GetTotalBonusHealth()
        {
            int total = 0;
            foreach (var item in _items) total += item.BonusHealth;
            return total;
        }

        public int GetTotalBonusDamage()
        {
            int total = 0;
            foreach (var item in _items) total += item.BonusDamage;
            return total;
        }

        public int GetTotalBonusArmor()
        {
            int total = 0;
            foreach (var item in _items) total += item.BonusArmor;
            return total;
        }

        public void Clear()
        {
            _items.Clear();
        }
        
    }
}