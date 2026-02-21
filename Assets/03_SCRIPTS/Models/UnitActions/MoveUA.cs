using System.Collections.Generic;
using MADP.Views;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Models.UnitActions
{
    public class MoveUA : BaseUnitAction
    {
        public UnitView UnitView { get; private set; }
        public List<Vector3> Path { get; private set; }
        public bool WillAttack { get; private set; }
        
        public MoveUA(UnitView unitView, List<Vector3> path, bool willAttack = false)
        {
            UnitView = unitView;
            Path = path;
            WillAttack = willAttack;
        }
    }
}