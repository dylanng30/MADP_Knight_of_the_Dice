using MADP.Models.CellEvents.Interfaces;
using MADP.Services.Gold.Interfaces;
using UnityEngine;

namespace MADP.Models.CellEvents
{
    public class HealCellEvent : ICellEvent
    {
        public HealCellEvent()
        {
            
        }
        public bool CanExecute(CellModel cell)
        {
            return cell.Attribute == CellAttribute.Heal;
        }

        public void Execute(UnitModel unit, CellModel cell)
        {
            int healAmount = 10;
            unit.Heal(healAmount);
            Debug.Log($"Unit {unit.Id} của tea {unit.TeamOwner} vừa được hồi {healAmount} máu");
        }
    }
}