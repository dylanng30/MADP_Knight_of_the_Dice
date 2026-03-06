using MADP.Models.CellEvents.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Services.VFX.Interfaces;
using MADP.Settings;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Models.CellEvents
{
    public class HealCellEvent : ICellEvent
    {
        private IVFXService _vfxService;
        public HealCellEvent(IVFXService vfxService)
        {
            _vfxService = vfxService;
        }
        public bool CanExecute(CellModel cell)
        {
            return cell.Attribute == CellAttribute.Heal;
        }

        public void Execute(UnitModel unit, UnitView unitView, CellModel cell, CellView cellView)
        {
            int healAmount = 10;
            unit.Heal(healAmount);
            _vfxService.PlayVFX(VFXType.Heal, cellView.GetUnitPosition());
            Debug.Log($"Unit {unit.Id} của tea {unit.TeamOwner} vừa được hồi {healAmount} máu");
        }
    }
}