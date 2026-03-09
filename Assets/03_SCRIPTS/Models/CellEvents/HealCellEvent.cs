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
            int baseAmount = 10;
            int randomBonus = Random.Range(0, 6);
            int healAmount = baseAmount + randomBonus;
            unit.Heal(healAmount);
            _vfxService.PlayVFX(VFXType.Heal, cellView.GetUnitPosition());

            NumberVFXPayload payload = new NumberVFXPayload(healAmount, Color.green, "+");
            Vector3 numberPos = cellView.GetUnitPosition() + Vector3.up * 2f;
            _vfxService.PlayVFX(VFXType.FloatingHealth, numberPos, payload);
        }
    }
}