using MADP.Models;
using MADP.Models.Inventory;
using MADP.Settings;

namespace MADP.Services.Inventory.Interfaces
{
    public interface IItemService
    {
        System.Action<UnitModel, ItemDataSO> OnItemEquipped { get; set; }
        bool TryEquipItem(PlayerInventoryModel playerInv, UnitModel targetUnit, ItemDataSO item);
    }
}