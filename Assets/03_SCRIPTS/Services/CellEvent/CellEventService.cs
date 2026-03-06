using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Models.CellEvents;
using MADP.Models.CellEvents.Interfaces;
using MADP.Services.CellEvent.Interfaces;
using MADP.Services.Gold.Interfaces;
using MADP.Services.VFX.Interfaces;

namespace MADP.Services.CellEvent
{
    public class CellEventService : ICellEventService
    {
        private readonly List<ICellEvent> _events;
        
        public CellEventService(IGoldService goldService, IVFXService vfxService)
        {
            _events = new List<ICellEvent>
            {
                new GoldCellEvent(goldService),
                new HealCellEvent(vfxService),
                new MythCellEvent(goldService),
                new HarmCellEvent()
            };
        }
        public ICellEvent GetEvent(CellModel cell)
        {
            return _events.FirstOrDefault(s => s.CanExecute(cell));
        }
    }
}