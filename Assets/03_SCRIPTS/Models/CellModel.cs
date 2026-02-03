using UnityEngine;

namespace MADP.Models
{
    public enum CellStructure
    {
        Normal, Spawn, Gate, Home
    }
    public enum CellAttribute
    {
        None, Red, Yellow, Purple, Blue, Green, Orange
    }
    public enum TeamColor
    {
        None, Red, Green, Blue, Yellow
    }
    public class CellModel
    {
        public int Index { get; private set; }
        
        public CellStructure Structure { get; private set; } = CellStructure.Normal;
        public CellAttribute Attribute { get; private set; } = CellAttribute.None;
        public TeamColor TeamOwner { get; private set; } = TeamColor.None;
        
        public UnitModel Unit { get; private set; }
        public bool HasUnit => Unit != null;

        public CellModel(int index, CellStructure structure, CellAttribute attribute, TeamColor teamOwner = TeamColor.None)
        {
            Index = index;
            Structure = structure;
            Attribute = attribute;
            TeamOwner = teamOwner;
        }

        public void Register(UnitModel unit)
        {
            if (Unit != null)
            {
                Debug.Log("Loi logic");
                return;
            }

            Unit = unit;
            //Debug.Log($"Unit {Unit.Id} cua team {Unit.TeamOwner.ToString()} vao o {Index}");
        }
        public void Clear()
        {
            Debug.Log($"Unit {Unit.Id} cua team {Unit.TeamOwner.ToString()} roi o {Index}");
            Unit = null;
        }
    }
}