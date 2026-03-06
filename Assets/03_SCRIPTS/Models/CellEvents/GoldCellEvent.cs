using MADP.Models.CellEvents.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Views;
using MADP.Views.Unit;
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

        public void Execute(UnitModel unit, UnitView unitView, CellModel cell, CellView cellView)
        {
            int bonusAmount = Random.Range(1, 6);
            if(unit.RoleType == RoleType.Miner)
                bonusAmount += 2;
            _goldService.AddGold(unit.TeamOwner, bonusAmount);
        }
    }
}