using System.Collections.Generic;

namespace MADP.Models.UnitActions
{
    public abstract class BaseUnitAction 
    {
        public List<BaseUnitAction> PreActions { get; private set; } = new ();
        public List<BaseUnitAction> PerformActions { get; private set; } = new ();
        public List<BaseUnitAction> PostActions { get; private set; } = new ();
    }
}

