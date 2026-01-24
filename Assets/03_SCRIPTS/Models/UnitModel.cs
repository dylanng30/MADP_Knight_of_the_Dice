using MADP.Models;

namespace MADP.Models
{
    public enum UnitState
    {
        InNest,
        Moving,
        InHome
    }
    public class UnitModel
    {
        public int Id { get; private set; }
        public TeamColor TeamOwner { get; private set; }
        public UnitStatModel Stat { get; private set; }
        public UnitState State { get; private set; } = UnitState.InNest;
        
        public int StepsMoved { get; private set; }
        public int CurrentIndex { get; private set; }

        public UnitModel(int id, TeamColor teamOwner)
        {
            Id = id;
            TeamOwner = teamOwner;
        }

        public void Reset()
        {
            State = UnitState.InNest;
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