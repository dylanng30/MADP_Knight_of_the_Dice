using MADP.Models;

namespace MADP.Models.CellEvents.Interfaces
{
    public interface ICellEvent
    {
        bool CanExecute(CellModel cell);
        void Execute(UnitModel unit, CellModel cell);
    }
}