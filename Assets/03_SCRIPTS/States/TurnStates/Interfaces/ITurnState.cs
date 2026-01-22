namespace MADP.States.TurnStates
{
    public interface ITurnState
    {
        void EnterTurn();
        void ExecuteTurn();
        void ExitTurn();
    }
}

