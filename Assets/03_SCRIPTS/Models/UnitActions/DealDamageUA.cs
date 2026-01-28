using System.Collections.Generic;
using UnityEngine;

namespace MADP.Models.UnitActions
{
    public class DealDamageUA : BaseUnitAction
    {
        public UnitModel TargetUnitModel;
        public StatModel TargetStatModel;
        public int Amount;
        public DealDamageUA(UnitModel targetUnitModel, int amount, List<Vector3> backPath)
        {
            TargetUnitModel = targetUnitModel;
            Amount = amount;
        }

        public DealDamageUA(UnitModel targetUnitModel, StatModel targetStatModel, int amount)
        {
            TargetUnitModel = targetUnitModel;
            TargetStatModel = targetStatModel;
            Amount = amount;
        }
    }
}