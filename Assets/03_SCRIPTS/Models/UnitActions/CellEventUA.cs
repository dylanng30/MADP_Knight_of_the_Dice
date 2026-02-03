using MADP.Models;
using MADP.Models.CellEvents.Interfaces;
using MADP.Views;
using UnityEngine;

namespace MADP.Models.UnitActions
{
    public class CellEventUA : BaseUnitAction
    {
        public UnitModel UnitModel { get; private set; }
        public UnitView UnitView { get; private set; }
        public CellModel CellModel { get; private set; }
        public CellView CellView { get; private set; }
        public ICellEvent CellEvent { get; private set; }

        public CellEventUA(UnitModel unitModel, UnitView unitView, CellModel cellModel, CellView cellView, ICellEvent cellEvent)
        {
            UnitModel = unitModel;
            UnitView = unitView;
            CellModel = cellModel;
            CellView = cellView;
            CellEvent = cellEvent;
        }
    }
}