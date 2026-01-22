using System.Collections.Generic;
using UnityEngine;

namespace MADP.Models.UnitActions
{
    public class DealDamageUA : BaseUnitAction
    {
        public UnitModel TargetUnitModel;
        public int Amount;
        public DealDamageUA(UnitModel targetUnitModel, int amount, List<Vector3> backPath)
        {
            TargetUnitModel = targetUnitModel;
            Amount = amount;
        }
    }
}