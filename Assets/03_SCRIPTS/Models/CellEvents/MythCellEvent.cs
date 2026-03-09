using MADP.Models.CellEvents.Interfaces;
using MADP.Models.VFX;
using MADP.Services.Gold;
using MADP.Services.Gold.Interfaces;
using MADP.Services.VFX.Interfaces;
using MADP.Settings;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Models.CellEvents
{
    public class MythCellEvent : ICellEvent
    {
        private GoldCellEvent _goldEvent;
        private HealCellEvent _healEvent;
        private HarmCellEvent _harmEvent;

        public MythCellEvent(GoldCellEvent goldEvent, HealCellEvent healEvent, HarmCellEvent harmEvent)
        {
            _goldEvent = goldEvent;
            _healEvent = healEvent;
            _harmEvent = harmEvent;
        }
        public bool CanExecute(CellModel cell)
        {
            return cell.Attribute == CellAttribute.Myth;
        }

        public void Execute(UnitModel unit, UnitView unitView, CellModel cell, CellView cellView)
        {
            int randomIndex = Random.Range(0, 3);
            switch (randomIndex)
            {
                case 0:
                    _goldEvent.Execute(unit, unitView, cell, cellView);
                    break;
                case 1:
                    _healEvent.Execute(unit, unitView, cell, cellView);
                    break;
                case 2:
                    _harmEvent.Execute(unit, unitView, cell, cellView);
                    break;
            }
        }
        
    }
}