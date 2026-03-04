using MADP.Models.CellEvents.Interfaces;
using MADP.Services.Gold.Interfaces;
using UnityEngine;

namespace MADP.Models.CellEvents
{
    public class GoldCellEvent : ICellEvent
    {
        private readonly IGoldService _goldService;

        public GoldCellEvent(IGoldService goldService)
        {
            _goldService = goldService;
        }
        public bool CanExecute(CellModel cell)
        {
            return cell.Attribute == CellAttribute.Gold;
        }

        public void Execute(UnitModel unit, CellModel cell)
        {
            int bonusAmount = Random.Range(1, 6);
            if(unit.RoleType == RoleType.Miner)
                bonusAmount += 2;
            _goldService.AddGold(unit.TeamOwner, bonusAmount);
        }
    }
}