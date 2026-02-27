using System.Collections.Generic;
using MADP.Models;
using MADP.Models.Inventory;
using MADP.Services.Gold.Interfaces;
using MADP.Settings;
using UnityEngine;

namespace MADP.Services.Shop
{
    public class ShopService
    {
        private readonly IGoldService _goldService;

        public ShopService(IGoldService goldService)
        {
            _goldService = goldService;
        }
        
        public bool TryBuyItem(ItemDataSO item, PlayerInventoryModel playerInventory)
        {
            if (item == null || playerInventory == null) return false;

            int currentGold = _goldService.GetGold(playerInventory.Team);
            if (currentGold < item.Price) return false;

            var paymentSuccess = playerInventory.TryAddItem(item);

            if (paymentSuccess)
            {
                //UI Noti
            }
            else
            {
                //UI Noti
            }
            
            return paymentSuccess;
        }
        
        public List<ItemDataSO> GenerateRandomShop(ShopDatabaseSO masterDB, int itemCount = 3)
        {
            List<ItemDataSO> result = new List<ItemDataSO>();
            
            if (masterDB == null || masterDB.Items.Count == 0) return result;

            List<ItemDataSO> pool = new List<ItemDataSO>(masterDB.Items);

            for (int i = 0; i < itemCount; i++)
            {
                if (pool.Count == 0) break;
                
                int randomIndex = Random.Range(0, pool.Count);
                result.Add(pool[randomIndex]);
                pool.RemoveAt(randomIndex); 
            }

            return result;
        }
    }
}