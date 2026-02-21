using System.Collections;
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
            Debug.Log(_turnController.CurrentTeam + " is now in Rolling State");
        }
        public override void ExecuteTurn()
        {
            if (_turnController.IsPlayerTurn && Input.GetKeyDown(KeyCode.Space))
            {
                _turnController.RollDice();
            }
            else if (!_turnController.IsPlayerTurn)
            {
                _turnController.RollDice();
            }
        }
    }
}

