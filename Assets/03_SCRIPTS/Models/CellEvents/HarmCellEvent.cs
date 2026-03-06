using MADP.Models.CellEvents.Interfaces;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Models.CellEvents
{
    public class HarmCellEvent : ICellEvent
    {
        public HarmCellEvent()
        {
            
        }
        public bool CanExecute(CellModel cell)
        {
            return cell.Attribute == CellAttribute.Harm;
        }

        public void Execute(UnitModel unit, UnitView unitView, CellModel cell, CellView cellView)
        {
            
        }
    }
}