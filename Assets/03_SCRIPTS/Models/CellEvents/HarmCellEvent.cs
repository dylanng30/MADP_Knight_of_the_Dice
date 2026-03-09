using MADP.Models.CellEvents.Interfaces;
using MADP.Models.VFX;
using MADP.Services.VFX.Interfaces;
using MADP.Settings;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Models.CellEvents
{
    public class HarmCellEvent : ICellEvent
    {
        private readonly IVFXService _vfxService;
        public HarmCellEvent(IVFXService vfxService)
        {
            _vfxService = vfxService;
        }
        public bool CanExecute(CellModel cell)
        {
            return cell.Attribute == CellAttribute.Harm;
        }

        public void Execute(UnitModel unit, UnitView unitView, CellModel cell, CellView cellView)
        {
            int baseAmount = 5;
            int randomBonus = Random.Range(0, 6);
            int damageAmount = baseAmount + randomBonus;
            unit.TakeDamage(damageAmount);
            
            NumberVFXPayload payload = new NumberVFXPayload(damageAmount, Color.red, "-");
            Vector3 numberPos = cellView.GetUnitPosition() + Vector3.up * 2f;
            _vfxService.PlayVFX(VFXType.FloatingHealth, numberPos, payload);
        }
    }
}