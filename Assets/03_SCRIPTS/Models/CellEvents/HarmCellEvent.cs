using MADP.Models.CellEvents.Interfaces;
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

        public void Execute(UnitModel unit, CellModel cell)
        {
            
        }
    }
}