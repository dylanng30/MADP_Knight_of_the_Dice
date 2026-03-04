using MADP.Models;
using MADP.Models.CellEvents.Interfaces;

namespace MADP.Services.CellEvent.Interfaces
{
    public interface ICellEventService
    {
        ICellEvent GetEvent(CellModel cell);
    }
}