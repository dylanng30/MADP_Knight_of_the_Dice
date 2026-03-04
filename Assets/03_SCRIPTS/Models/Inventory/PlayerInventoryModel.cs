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

        public bool TryAddItem(ItemDataSO item)
        {
            if(Items.Count < ItemSlots) return false;
            Items.Add(item);
            return true;
        }
        
    }
}