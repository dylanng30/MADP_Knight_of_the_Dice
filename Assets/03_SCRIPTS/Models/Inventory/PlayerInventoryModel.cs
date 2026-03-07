using System;
using System.Collections.Generic;
using MADP.Settings;
using UnityEditor;

namespace MADP.Models.Inventory
{
    public class PlayerInventoryModel
    {
        public const int ItemSlots = 10;
        public List<ItemDataSO> Items = new();
        public TeamColor Team;
        
        public Action OnInventoryUpdated;

        public PlayerInventoryModel(TeamColor team)
        {
            Team = team;
        }

        public bool TryAddItem(ItemDataSO item)
        {
            if(Items.Count >= ItemSlots) return false;
            Items.Add(item);
            OnInventoryUpdated?.Invoke();
            return true;
        }
        
        public bool RemoveItem(ItemDataSO item)
        {
            if (Items.Contains(item))
            {
                Items.Remove(item);
                OnInventoryUpdated?.Invoke();
                return true;
            }
            return false;
        }
        
    }
}