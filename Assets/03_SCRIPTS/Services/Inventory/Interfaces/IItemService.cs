using MADP.Models;
using MADP.Models.Inventory;
using MADP.Settings;

namespace MADP.Services.Inventory.Interfaces
{
    public interface IItemService
    {
        bool TryEquipItem(PlayerInventoryModel playerInv, UnitModel targetUnit, ItemDataSO item);
    }
}