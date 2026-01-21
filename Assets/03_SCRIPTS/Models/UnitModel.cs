using MADP.Models;

namespace MADP.Models
{
    public enum UnitType
    {
        One, Two, Three, Four
    }

    public enum UnitState
    {
        InCage,
        Moving,
        InHome
    }
    public class UnitModel
    {
        public UnitType Type { get; private set; } = UnitType.One;
        public TeamColor TeamOwner { get; private set; }
        public UnitStatModel Stat { get; private set; }
        public UnitState State { get; private set; } = UnitState.InCage;
        
        public int StepsMoved { get; private set; }
        public int CurrentIndex { get; private set; }

        public UnitModel(UnitType type, TeamColor teamOwner)
        {
            Type = type;
            TeamOwner = teamOwner;
        }

        public void Reset()
        {
            State = UnitState.InCage;
            StepsMoved = 0;
            CurrentIndex = -1;
        }

        public void MoveTo(int cellIndex)
        {
            CurrentIndex = cellIndex;
        }

        public void AddSteps(int amount)
        {
            StepsMoved += amount;
        }

        public void SetState(UnitState newState)
        {
            State = newState;
        }
    }
}