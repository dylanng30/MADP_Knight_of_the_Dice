using System.Collections.Generic;
using MADP.Views.Unit;
using UnityEngine;

namespace MADP.Models.UnitActions
{
    public class MoveUA : BaseUnitAction
    {
        public UnitView UnitView { get; private set; }
        public List<Vector3> Path { get; private set; }
        public Vector3 DefaultDirection { get; private set; }
        
        public MoveUA(UnitView unitView, List<Vector3> path, Vector3 defaultDirection = default)
        {
            UnitView = unitView;
            Path = path;
            DefaultDirection = defaultDirection;
        }
    }
}