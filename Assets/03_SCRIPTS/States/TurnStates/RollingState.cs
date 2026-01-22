using MADP.Controllers;
using UnityEngine;

namespace MADP.States.TurnStates
{
    public class RollingState  : BaseTurnState
    {
        public RollingState(TurnController controller) : base(controller)
        {
        }
        public override void EnterTurn()
        {
            base.EnterTurn();
            if (_turnController.IsPlayerTurn)
            {
                //_turnController.ShowDiceView(true);
            }
            else
            {
                // Nếu là Bot -> Tự động Roll sau vài giây (Implement sau)
            }
        }
        public override void ExecuteTurn()
        {
            if (_turnController.IsPlayerTurn && Input.GetKeyDown(KeyCode.Space))
            {
                _turnController.RollDice();
            }
        }
        public override void ExitTurn()
        {
            base.ExitTurn();
            //_turnController.ShowDiceView(false);
        }
    }
}

