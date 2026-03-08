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
            if (currentGold < item.Price)
            {
                Debug.Log("Khong du tien");
                return false;
            }

            var paymentSuccess = playerInventory.TryAddItem(item);

            if (paymentSuccess)
            {
                //UI Noti
                _goldService.TrySpendGold(playerInventory.Team, item.Price);
                Debug.Log("Mua thành công");
            }
            else
            {
                Debug.Log("Het cho");
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
        
        /*public void ProcessBotShopping(LobbySlotModel botSlot, List<ItemDataSO> shopItems)
        {
            if (botSlot.PlayerType != PlayerType.Bot || botSlot.Inventory == null) return;

            foreach (var item in shopItems)
            {
                int currentGold = _goldService.GetGold(botSlot.TeamColor);
                if (currentGold >= item.Price && botSlot.Inventory.Items.Count < PlayerInventoryModel.ItemSlots)
                {
                    _goldService.TrySpendGold(botSlot.TeamColor, item.Price);
                    botSlot.Inventory.TryAddItem(item);
                    Debug.Log($"Bot {botSlot.TeamColor} đã mua: {item.ItemName}");
                }
            }
        }*/
    }
}