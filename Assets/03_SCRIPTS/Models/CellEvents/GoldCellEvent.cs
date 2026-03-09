using MADP.Models.CellEvents.Interfaces;
using MADP.Models.VFX;
using MADP.Services.Gold.Interfaces;
using MADP.Services.VFX.Interfaces;
using MADP.Settings;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Models.CellEvents
{
    public class GoldCellEvent : ICellEvent
    {
        private readonly IGoldService _goldService;
        private readonly IVFXService _vfxService;

        public GoldCellEvent(IGoldService goldService, IVFXService vfxService)
        {
            _goldService = goldService;
            _vfxService = vfxService;
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
            
            NumberVFXPayload payload = new NumberVFXPayload(bonusAmount, Color.yellow, "+");
            Vector3 numberPos = cellView.GetUnitPosition() + Vector3.up * 2f;
            _vfxService.PlayVFX(VFXType.FloatingHealth, numberPos, payload);
        }
    }
}