using MADP.Models;
using MADP.Models.Inventory;
using MADP.Services.Inventory.Interfaces;
using MADP.Settings;
using UnityEngine;

namespace MADP.Services.Inventory
{
    public class ItemService : IItemService
    {
        public bool TryEquipItem(PlayerInventoryModel playerInv, UnitModel targetUnit, ItemDataSO item)
        {
            if (playerInv == null || targetUnit == null || item == null) return false;
            
            if (!playerInv.Items.Contains(item))
            {
                Debug.LogWarning($"Không tìm thấy {item.ItemName} trong túi đồ!");
                return false;
            }
            
            bool isAdded = targetUnit.Inventory.AddItem(item);

            if (isAdded)
            {
                playerInv.RemoveItem(item);
                return true;
            }

            return false;
        }
    }
}