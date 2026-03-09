using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Models.CellEvents;
using MADP.Models.CellEvents.Interfaces;
using MADP.Models.VFX;
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
            GoldCellEvent goldCellEvent = new GoldCellEvent(goldService, vfxService);
            HarmCellEvent harmCellEvent = new HarmCellEvent(vfxService);
            HealCellEvent healCellEvent = new HealCellEvent(vfxService);
            
            MythCellEvent mythCellEvent = new MythCellEvent(goldCellEvent, healCellEvent, harmCellEvent);
            
            _events = new List<ICellEvent>
            {
                goldCellEvent, harmCellEvent, healCellEvent, mythCellEvent,
            };
        }
        public ICellEvent GetEvent(CellModel cell)
        {
            return _events.FirstOrDefault(s => s.CanExecute(cell));
        }
        
    }
}