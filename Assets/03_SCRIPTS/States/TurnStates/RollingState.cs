using MADP.Controllers;
using MADP.States.TurnStates.Interfaces;

namespace MADP.States.TurnStates
{
    public class WaitingForDiceRollState  : ITurnState
    {
        private TurnController _controller;
        
        public WaitingForDiceRollState(TurnController controller)
        {
            _controller = controller;
        }
        public void EnterTurn()
        {
            
        }

        public void ExecuteTurn()
        {

        }

        public void ExitTurn()
        {
            
        }
    }
}

