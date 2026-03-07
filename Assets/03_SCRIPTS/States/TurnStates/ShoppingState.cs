using MADP.Controllers;

namespace MADP.States.TurnStates
{
    public class ShoppingState : BaseTurnState
    {
        public ShoppingState(TurnController controller) : base(controller)
        {
        }

        public override void EnterTurn()
        {
            base.EnterTurn();
            
            _turnController.SetRollButtonVisibility(false);
            _turnController.SetEndTurnButtonVisibility(false);
            
            _turnController.StartShoppingPhase();
        }
    }
}