using MADP.Models;
using MADP.Views;
using MADP.Views.Unit;

namespace MADP.Models.CellEvents.Interfaces
{
    public interface ICellEvent
    {
        bool CanExecute(CellModel cell);
        void Execute(UnitModel unit, UnitView unitView, CellModel cell, CellView cellView);
    }
}